/*
 * PlayerController.cs - ZForward
 * @version: 1.1.0
*/

using System;
using UnityEngine;
using UnityEngine.Serialization;
using Humanoid_Basics.Camera;

namespace Humanoid_Basics.Player
{
    public class PlayerController : Singleton<PlayerController>
    {
        [Header("Humanoid Core")]
        public HumanoidCore humanoidCore;
        public FP_Input playerInput;
        
        [Header("Humanoid Plugins")]
        public HumanoidEquipment humanoidEquipment;
        public HumanoidPickup humanoidPickup;

        [HideInInspector]
        public CameraCore cameraCore;

        [Header("Options")]
        public bool canSwitchAimSide = true;
        public bool autoLean = true;
        public AimMode aimMode = AimMode.Hold;
        public float aimCameraDifference = 0.3f;
        public float swimCameraDifference = -0.3f;
        public float aimIsRightSide;
        public bool useDebug;

        // Offset Y
        public float targetOffsetYGrounded = 1.5f;
        public float targetOffsetYSwimming = 2.5f;
        
        public RunMode runMode = RunMode.Hold;

        // Keyboard
        [Header("Controls")]
        [FormerlySerializedAs("JumpKey")] public KeyCode jumpKey = KeyCode.Space;
        [FormerlySerializedAs("RunKey")] public KeyCode runKey = KeyCode.LeftShift;
        [FormerlySerializedAs("CrouchKey")] public KeyCode crouchKey = KeyCode.C;
        [FormerlySerializedAs("ShootKey")] public KeyCode shootKey = KeyCode.Mouse0;
        [FormerlySerializedAs("AimKey")] public KeyCode aimKey = KeyCode.Mouse1;
        [FormerlySerializedAs("SwitchAimSideKey")] public KeyCode switchAimSideKey = KeyCode.T;
        [FormerlySerializedAs("ReloadKey")] public KeyCode reloadKey = KeyCode.R;
        [FormerlySerializedAs("PickUpWeaponKey")] public KeyCode pickUpWeaponKey = KeyCode.E;
        [FormerlySerializedAs("EquipWeaponKey")] public KeyCode equipWeaponKey = KeyCode.Tab;
        public float mouseSensitivity = 3;
        
        //
        public Vector2 movementAxis;
        public Vector2 cameraAxis;

        [HideInInspector]
        public KeyCode[] keyCodes = {
            KeyCode.Alpha1,
            KeyCode.Alpha2,
            KeyCode.Alpha3,
            KeyCode.Alpha4,
            KeyCode.Alpha5,
            KeyCode.Alpha6,
            KeyCode.Alpha7,
            KeyCode.Alpha8,
            KeyCode.Alpha9,
        };

        // Plugins
        private bool loadedEquipmentPlugin;
        private bool loadedPickupPlugin;
        private bool sideCollision, currentSideCollision, oppositeSideCollision;
        public float originalSide;
        
        public enum AimMode
        {
            Hold,
            Toggle
        }
        
        public enum RunMode
        {
            Hold,
            Toggle
        }

        private void Start()
        {
            loadedEquipmentPlugin = humanoidEquipment != null;
            if (loadedEquipmentPlugin)
            {
                Debug.Log("[PlayerController] Loaded Equipment Plugin");
            }
            
            loadedPickupPlugin = humanoidPickup != null;
            if (loadedPickupPlugin)
            {
                Debug.Log("[PlayerController] Loaded Pickup Plugin");
            }

            cameraCore = CameraCore.Instance;
            originalSide = cameraCore.targetOffset.x;
        }

        private void Update()
        {
            if (!Application.isPlaying) { return; }
            
            if (!humanoidCore.isAttacking)
            {
                if (playerInput.UseMobileInput)
                {
                    var input = playerInput.MoveInput();
                    input.Normalize();
                    input.x = playerInput.MovePressed() ? input.x : 0;
                    input.z = playerInput.MovePressed() ? input.z : 0;

//#if UNITY_EDITOR
//                    var keyboardX = Input.GetAxisRaw("Horizontal");
//                    var keyboardY = Input.GetAxisRaw("Vertical");
//                    if (keyboardX != 0) input.x = keyboardX;
//                    if (keyboardY != 0) input.z = keyboardY;
//#endif

                    movementAxis.x = input.x;
                    movementAxis.y = input.z;

                    humanoidCore.SetXAxis(input.x);
                    humanoidCore.SetYAxis(input.z);

                    // Update Camera 2.0
                    cameraCore.SetCameraX(playerInput.LookInput().x * mouseSensitivity);
                    cameraCore.SetCameraZ(playerInput.LookInput().y * -mouseSensitivity);
                }
                else
                {
                    movementAxis.x = Input.GetAxisRaw("Horizontal");
                    movementAxis.y = Input.GetAxisRaw("Vertical");
                    humanoidCore.SetXAxis(Input.GetAxisRaw("Horizontal"));
                    humanoidCore.SetYAxis(Input.GetAxisRaw("Vertical"));

                    // Update Camera 2.0
                    if (Cursor.lockState == CursorLockMode.Locked)
                    {
                        cameraCore.SetCameraX(Input.GetAxisRaw("Mouse X") * mouseSensitivity);
                        cameraCore.SetCameraZ(Input.GetAxisRaw("Mouse Y") * -mouseSensitivity);
                    }
                }
            }
            else
            {
                humanoidCore.SetXAxis(0);
                humanoidCore.SetYAxis(0);

                // Update Camera 2.0
                cameraCore.SetCameraX(0);
                cameraCore.SetCameraZ(0);
            }
            HandleCameraTarget();
            HandleKeyPress();

            
            // Debug Aim
            if (useDebug && humanoidCore.isAiming && humanoidCore.equippedWeapon)
            {
                var cameraTransform = cameraCore.cameraObject.transform;
                var cameraPosition = cameraTransform.position;
                var cameraForward = cameraTransform.forward;
                var color = Color.yellow;
                Physics.Raycast(cameraPosition, cameraForward, out var centerHit);
                Debug.DrawLine(cameraPosition, cameraPosition + cameraForward * (100f), color);                        
                Debug.DrawRay(humanoidCore.currentWeapon.barrel.position, centerHit.point  - humanoidCore.currentWeapon.barrel.transform.position, color);
            }
        }

        private void FixedUpdate()
        {
            if (!Application.isPlaying) { return; }

            if (humanoidCore.humanoidType == HumanoidCore.Type.Npc) return;
            if (humanoidCore.humanoidStatus == HumanoidCore.Status.Dead) { return; }

            PlayerMovement();

            if (humanoidCore.isAiming) 
            {
                var cameraTransform = cameraCore.cameraObject.transform;
                var cameraTransformForward = cameraTransform.forward;
                var spineOffset = cameraTransformForward - cameraTransform.up / 5;
                var armsOffset = cameraTransformForward;
    
                
                // Ik Head Look At Position
                if (humanoidCore.playerFeatures.canMeleeAttack && !humanoidCore.equippedWeapon)
                {
                    humanoidCore.ikControl.lookAtPosition = humanoidCore.ikControl.head.position + cameraTransform.right * humanoidCore.advancedSettings.meleeAimOffset;
                }
                else
                {
                    humanoidCore.ikControl.lookAtPosition = humanoidCore.ikControl.head.position + humanoidCore.aimHelper.forward;
                }
                
                // if (cameraCore.targetOffset.x <= 0.0f)
                // {
                //     armsOffset -= cameraTransform.right / 4 ;
                // }
                spineOffset.y = Mathf.Clamp(spineOffset.y, -0.2f, 0.35f);
                
                if (humanoidCore.SomethingInFront())
                {
                    spineOffset.y = Mathf.Clamp(spineOffset.y, 0, 0f);
                    armsOffset.y = Mathf.Clamp(armsOffset.y, 0, 1);
                }
                
                // Auto Lean
                if (autoLean)
                {
                    if (SomethingInFrontAim(2) && humanoidCore.currentWeapon && !humanoidCore.currentWeapon.reloadProgress)
                    {
                        if (cameraCore.targetOffset.x > 0.0f)
                        {
                            humanoidCore.lean = -.2f;
                        }
                        else
                        {
                            humanoidCore.lean = .2f;
                        }
                    }
                    else
                    {
                        humanoidCore.lean = 0;
                    }
                }
                
                if (humanoidCore.crouch)
                {
                    armsOffset.y = Mathf.Clamp(armsOffset.y, -.7f, 0.4f);
                    if (cameraCore.targetOffset.x <= 0.0f)
                    {
                        spineOffset -= cameraTransform.right * .3f;
                    }
                    else
                    {
                        spineOffset += cameraTransform.right * .3f;
                    }
                    if (!humanoidCore.SomethingInFront())
                    {
                        spineOffset.y = Mathf.Clamp(spineOffset.y, -.5f, -.5f);
                    }
                }

                var transformRotatePosition = humanoidCore.transformToRotate.position;
                if (cameraCore.targetOffset.x <= 0.0f)
                {
                    //armsOffset.x *= cameraCore.targetOffset.x;
                }
                else
                {
                    //armsOffset.x *= -cameraCore.targetOffset.x;
                }
                
                var lookRotation = new Vector3(Input.GetAxisRaw("Mouse X") * mouseSensitivity, 0 ,0);

                // We need the lerp right now as it causes other issues without it.
                humanoidCore.aimRotationAux = Quaternion.Lerp(humanoidCore.aimRotationAux, Quaternion.LookRotation(transformRotatePosition + armsOffset + cameraTransform.up * humanoidCore.recoil / 10 - transformRotatePosition), 15f * Time.fixedDeltaTime);
                
                // humanoidCore.aimRotationAux = Quaternion.LookRotation(
                //      (humanoidCore.transformToRotate.position + armsOffset +
                //       cameraTransform.up * humanoidCore.recoil / 10) - humanoidCore.transformToRotate.position);
                
                if (humanoidCore.playerFeatures.canMeleeAttack && !humanoidCore.equippedWeapon) return;
                
                // Moving Spine - Look around while aiming (no weapon)
                humanoidCore.aimRotationSpineAux = Quaternion.Lerp(humanoidCore.aimRotationSpineAux, Quaternion.LookRotation(transformRotatePosition + spineOffset + cameraTransform.up * humanoidCore.recoil / 5 - transformRotatePosition) * new Quaternion(0, 0.5f, humanoidCore.lean, 1) * humanoidCore.startSpineRot, 15f * Time.fixedDeltaTime);
            }
            else
            {
                humanoidCore.lean = 0;

                // Adjust Target Offset
                cameraCore.targetOffset.y = Mathf.Lerp(cameraCore.targetOffset.y, humanoidCore.isSwimming ? targetOffsetYSwimming : targetOffsetYGrounded, 6f * Time.fixedDeltaTime);
                
                // Ik Head Look At Position
                humanoidCore.ikControl.lookAtPosition = humanoidCore.ikControl.head.position + cameraCore.cameraObject.transform.forward;
                
                humanoidCore.aimRotationSpineAux = Quaternion.Lerp(humanoidCore.aimRotationSpineAux, humanoidCore.aimHelperSpine.rotation, 20 * Time.fixedDeltaTime);

                Vector3 off;
                switch (humanoidCore.advancedSettings.spineFacingDirection)
                {
                    case HumanoidCore.Direction.Forward:
                        off = humanoidCore.aimHelperSpine.forward;
                        break;
                    case HumanoidCore.Direction.Back:
                        off = -humanoidCore.aimHelperSpine.forward;
                        break;
                    case HumanoidCore.Direction.Up:
                        off = humanoidCore.aimHelperSpine.up;
                        break;
                    case HumanoidCore.Direction.Down:
                        off = -humanoidCore.aimHelperSpine.up;
                        break;
                    case HumanoidCore.Direction.Left:
                        off = -humanoidCore.aimHelperSpine.right;
                        break;
                    case HumanoidCore.Direction.Right:
                        off = humanoidCore.aimHelperSpine.right;
                        break;
                    default:
                        off = Vector3.zero;
                        break;
                }

                off.y = Mathf.Clamp(off.y, 0, 5);
                if (humanoidCore.crouch)
                {
                    off -= humanoidCore.transformToRotate.right * 0.3f;
                }
            
                humanoidCore.aimRotationAux = Quaternion.Lerp(humanoidCore.aimRotationAux, Quaternion.LookRotation((humanoidCore.aimHelper.position + off) - humanoidCore.aimHelper.position), 10 * Time.deltaTime);

            }

            if (humanoidCore.playerAnimator.enabled)
                humanoidCore.aimHelperSpine.rotation = humanoidCore.aimRotationSpineAux;

        }

        private void PlayerMovement()
        {
            if (humanoidCore.humanoidStatus == HumanoidCore.Status.Dead) { return; }

            var cameraObject = cameraCore.cameraObject;
            var cameraObjectTransform = cameraObject.transform;
            var cameraObjectTransformForward = cameraObjectTransform.forward;
                
            // As this is camera related i think this needs to be moved...
            var orientedX = movementAxis.x * cameraObjectTransform.right;
            var orientedY = movementAxis.y * cameraObjectTransformForward;

            orientedX.y = 0;
            orientedY.y = 0;

            humanoidCore.moveAxis = orientedY + orientedX;

            if (/* (humanoidCore.inMoveState || humanoidCore.isSwimming) &&*/ !humanoidCore.isClimbing)
            {
                var lookForward = cameraObjectTransformForward;

                if (humanoidCore.isAiming)
                {
                    // if (cameraCore.targetOffset.x <= 0.0f && humanoidCore.crouch)
                    // {
                    //     lookForward -= cameraObject.transform.right / 2;
                    // }
                    //
                    // var centerHitted = Physics.Raycast(CameraCore.Instance.cameraObject.transform.position, CameraCore.Instance.cameraObject.transform.forward, out var centerHit);
                    // if (centerHitted)
                    // {
                    //     lookForward = centerHit.transform.forward;
                    // }
                    lookForward.y = 0;
                    // cameraCore.SetCameraX();
                    // cameraCore.SetCameraZ(Input.GetAxisRaw("Mouse Y") * -mouseSensitivity);
                    //lookForward.x += Input.GetAxisRaw("Mouse X") * mouseSensitivity;
                    
                    var lookRotation = new Vector3(Input.GetAxisRaw("Mouse X") * mouseSensitivity, 0 ,0);

                    // Get the new rotation based on the camera looking forward
                    humanoidCore.rotationAux = Quaternion.LookRotation((humanoidCore.transformToRotate.position + lookForward) - humanoidCore.transformToRotate.position);
                    // humanoidCore.rotationAux = Quaternion.LookRotation((humanoidCore.transformToRotate.position + lookForward) - humanoidCore.transformToRotate.position);
                }
                
                // Lets rotate the player animator to the new position if aiming.
                humanoidCore.transformToRotate.rotation = Quaternion.Lerp(humanoidCore.playerAnimator.transform.rotation, humanoidCore.rotationAux, 10f * Time.deltaTime);

                // If we are moving and not aiming then calculate rotation for the next frame...
                if (humanoidCore.moveAxis != Vector3.zero)
                {
                    if (!humanoidCore.isAiming)
                    {
                        humanoidCore.rotationAux = Quaternion.LookRotation((humanoidCore.transformToRotate.position + humanoidCore.moveAxis) - humanoidCore.transformToRotate.position);
                    }
                }

            }
        }

        public void ForceRotate(Transform targetTransform)
        { 
            Quaternion targetRotation = targetTransform.rotation;
             
            Quaternion rotationWithOffset = targetRotation * Quaternion.Euler(0, 180, 0);
             
            humanoidCore.rotationAux = rotationWithOffset;
            humanoidCore.transformToRotate.rotation = humanoidCore.rotationAux;
        }

        public void ForceMove(Transform targetTransform)
        { 
            humanoidCore.playerAnimator.gameObject.transform.position = targetTransform.position;
        }

        private void HandleCameraTarget()
        {
            // Camera Parent Transform
            var cameraTransform = cameraCore.cameraObject.transform;
            var cameraTransformForward = cameraTransform.forward;
            var cameraTransformRight = cameraTransform.right;
            
            // Collision aim detection
            var startPoint = cameraCore.cameraPivot[0].position;
            currentSideCollision = Physics.SphereCast(startPoint, .2f, -cameraTransformForward + cameraTransformRight * (cameraCore.useTargetOffset?(cameraCore.targetOffset.x * 1):0), out _, cameraCore.targetDistance / 2);
            oppositeSideCollision = Physics.SphereCast(startPoint, .2f, -cameraTransformForward + cameraTransformRight * (cameraCore.useTargetOffset?(cameraCore.targetOffset.x * -1):0), out _, cameraCore.targetDistance / 2);
            sideCollision = humanoidCore.isAiming && currentSideCollision;
            
            // Check for collision while aiming
            if (sideCollision && !oppositeSideCollision)
            {
                originalSide = cameraCore.targetOffset.x;
                cameraCore.targetOffset.x *= -1;
            }
            else if(!humanoidCore.isAiming) 
            {
                cameraCore.targetOffset.x = originalSide;
            }
            
            // Adjust Camera if we are pressing Aim
            if (humanoidCore.isAiming)
            {
                cameraCore.SetTargetDistanceModifier(aimCameraDifference);
            } 
            else if (humanoidCore.isSwimming)
            {
                cameraCore.SetTargetDistanceModifier(swimCameraDifference);
            }
            else
            {
                cameraCore.SetTargetDistanceModifier(0);
            }

            // Check for ragdoll, if so lock camera to root bone, if not back to animator
            if (humanoidCore.ragdollHelper.ragdolled)
            {
                if (cameraCore.GetTarget() == humanoidCore.boneRb[0].transform) return;
                cameraCore.SetTarget(humanoidCore.boneRb[0].transform);
                cameraCore.useTargetOffset = false;
            }
            else
            {
                if (cameraCore.GetTarget() == humanoidCore.playerAnimator.transform) return;
                cameraCore.SetTarget(humanoidCore.playerAnimator.transform);
                cameraCore.useTargetOffset = true;
            }

        }

        private void HandleKeyPress()
        {
            
            /////////////////////
            // Camera Controls //
            /////////////////////
            
            // Switch Camera Sides
            if (Input.GetKeyDown(switchAimSideKey) && canSwitchAimSide)
            {
                SwitchCameraSide();
            }

            ///////////////////////
            // Humanoid Controls //
            ///////////////////////

            // Handle Jump & Standing
            if (playerInput.UseMobileInput ? playerInput.Jump() : Input.GetKeyDown(jumpKey))
            {
                humanoidCore.Jump();
                humanoidCore.SetStand();
            }
            
            //// Handle Crouch
            //if (Input.GetKeyDown(crouchKey))
            //{
            //    humanoidCore.SetCrouch();
            //    cameraCore.SetTargetOffsetModifier(humanoidCore.crouch ? new Vector2(0, -0.5f) : new Vector2(0, 0f));
            //}
            
            // Handle Weapon Switch
            for (var i = 0; i < keyCodes.Length; i++)
            {
                if (!Input.GetKeyDown(keyCodes[i])) continue;
                humanoidCore.SwitchWeapon(i);
            }
            
            // Run
            if (runMode == RunMode.Hold)
            {
                humanoidCore.PlayerRun(Input.GetKey(runKey));
            }
            else if (Input.GetKeyDown(runKey) && runMode == RunMode.Toggle)
            {
                humanoidCore.ToggleRun();
            }
            
            //// Aiming
            //if (aimMode == AimMode.Hold)
            //{
            //    humanoidCore.SetAim(Input.GetKey(aimKey) || (Input.GetKey(shootKey) && humanoidCore.equippedWeapon));
            //} 
            //else if (Input.GetKeyDown(aimKey) && aimMode == AimMode.Toggle)
            //{
            //    humanoidCore.ToggleAim();
            //}

            //// Toggle Weapon
            //if (Input.GetKeyDown(equipWeaponKey))
            //{
            //    humanoidCore.ToggleWeapon();
            //}            
            
            //// Shoot
            //if (Input.GetKey(shootKey) && humanoidCore.equippedWeapon)
            //{
            //    humanoidCore.UseWeapon();
            //}
            //else if (Input.GetKeyDown(shootKey) && !humanoidCore.equippedWeapon)
            //{
            //    humanoidCore.UseWeapon();
            //}  
            
            // Reload
            if (Input.GetKeyDown(reloadKey))
            {
                humanoidCore.ReloadWeapon();
            }

            // Pick Up Item
            if (loadedPickupPlugin)
            {
                if (!humanoidPickup.automaticPickup && Input.GetKeyDown(pickUpWeaponKey))
                {
                    humanoidPickup.PickUpItem();
                }
            }

        }

        private bool SomethingInFrontAim(float distance)
        {
            var cameraObject = cameraCore.cameraObject;
            var cameraObjectTransform = cameraObject.transform;
            var camF = cameraObjectTransform.forward;
            var camRight = cameraObjectTransform.right;
            camF.y = 0;
            var offset = cameraCore.targetOffset.x <= 0.0f ? camRight * 0.15f : -camRight * 0.15f;
            var posToDetect = humanoidCore.transformToRotate.position + humanoidCore.transformToRotate.up * .5f;
            return Physics.Raycast(posToDetect, camF + offset, distance) && !Physics.Raycast(posToDetect + (offset * -5), camF + (offset * -5), distance);
        }

        private void SwitchCameraSide()
        {
            if (oppositeSideCollision) return;
            var cachePosition = cameraCore.transform.position;
            cameraCore.targetOffset.x *= -1;
            originalSide = cameraCore.targetOffset.x;
            cameraCore.transform.LookAt(cachePosition);
        }
        
    }
}
