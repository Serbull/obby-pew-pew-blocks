/*
 * HumanoidCore.cs - ZForward
 * Core script, this can be managed by PlayerController or NpcController
 * @version: 1.1.0
*/

using System;
using System.Collections;
using System.ComponentModel;
using Humanoid_Basics.Weapon;
using UnityEngine;
using UnityEngine.Serialization;

namespace Humanoid_Basics.Player
{
    [RequireComponent(typeof(HumanoidInventory))]
    
    public class HumanoidCore : MonoBehaviour
    {
        public const string Version = "1.1.0";
        
        #region Enumerators

        public enum Direction
        {
            Forward,
            Back,
            Up,
            Down,
            Left,
            Right
        }
        
        public enum Type
        {
            Player,
            Npc,
        }
        
        public enum Status
        {
            Active,
            Dead
        }

        #endregion
        
        #region Structures
        
        [Serializable]
        public sealed class PlayerSettings
        {
            public float baseGravity = -9.81f;
            public float crouchSpeed = 1f;
            public float walkSpeed = 2.3f;
            public float runSpeed = 4.6f;
            public float aimWalkSpeed = 1.5f;
            public float jumpForce = 7;
            
            [Range(0, 2)] public float crouchHeight = 0.75f;
        }

        [Serializable]
        public sealed class PlayerFeatures
        {
            public bool useGravity = true;
            public bool ragdollWhenFall = true;
            public bool canMeleeAttack = true;
            public bool canJump = true;
            public bool canCrouch = true;
        }

        [Serializable]
        public sealed class PlayerAdvancedSettings
        {
            [Header("Melee")]
            public float meleeAimOffset = -20f;
            
            [Header("Weapons")]
            public float switchWeaponTime = .5f;
            [Range(-1, 1)] public float bellyOffset;
            public Direction spineFacingDirection;
            
            [Header("Swimming")]
            public float swimSpeed = 2.0f;
            public float swimFastSpeed = 3.0f;
            public float swimWaterLevel = 4.5f;
            public float swimFloatHeight = 0.45f;
            public Vector3 swimBuoyancyCentreOffset;
            public float swimBounceDamp = 2f;
            public AudioClip swimAudio;
            public AudioClip splashAudio;
        }

        public sealed class PlayerAudio
        {
            
        }
        
        #endregion

        #region Public Variables
        
        [Header("Humanoid Settings")]
        
        [Description("If the Humanoid is Player Controlled or AI.")]
        public Type humanoidType = Type.Player;
        
        [Description("Current Status of the Humanoid.")]
        public Status humanoidStatus = Status.Active;
        
        [Description("What layers are considered ground layers.")]
        public LayerMask groundLayers;
        public LayerMask frontLayers;

        [Header("Player Settings")]
        public PlayerSettings playerSettings = new PlayerSettings();
        
        [Header("Player Features")]
        public PlayerFeatures playerFeatures = new PlayerFeatures();        
        
        [Header("Advanced Settings")]
        public PlayerAdvancedSettings advancedSettings = new PlayerAdvancedSettings();



        public event Action OnSwimEnter;
        public event Action OnSwimExit;


        [Range(0, 5)] [SerializeField] private float heightFromGroundRaycast = 5f;
        [Range(0, 2)] [SerializeField] private float raycastDownDistance = 0.35f;
        [SerializeField] public LayerMask detectableLayers;
        
        // Upwards Sensor
        public LayerMask upwardsSensorLayerMask;
        [HideInInspector] public bool hasUpwardsSensorHit;
        private RaycastHit upwardsSensorHit;
        
        // Front Sensor
        [HideInInspector] public bool hasFrontSensorHit;
        private RaycastHit frontSensorHit;
        
        // Front Sensor
        [HideInInspector] public bool hasOutwardSensorHit;
        private RaycastHit outwardSensorHit;
        
        // Floor Sensor
        [HideInInspector] public bool hasDetectableHit;
        public RaycastHit detectableHit;
        
        #endregion
        
        #region Hidden, Private & Unsorted
        
        [HideInInspector]
        public Transform aimHelper, aimHelperSpine;
        
        [HideInInspector]
        public HumanoidInventory humanoidInventory;

        [HideInInspector]
        public Animator playerAnimator;
        
        [HideInInspector]
        public RagdollHelper ragdollHelper;

        [HideInInspector]
        public Rigidbody rb;
        
        [HideInInspector]
        public CapsuleCollider capsuleCollider;
        
        [HideInInspector]
        public IKControl ikControl;

        [HideInInspector]
        public TransformPathMaker pathMaker;

        [HideInInspector]
        public Transform transformToRotate;

        [HideInInspector]
        public Vector3 moveAxis;

        [HideInInspector]
        public Rigidbody[] boneRb;

        [HideInInspector]
        public Transform hipsParent;

        [HideInInspector]
        public bool isAttacking;

        [HideInInspector]
        public bool crouch;
        [FormerlySerializedAs("aim")]
        public bool isAiming;

        [HideInInspector]
        public AudioSource audioSource;

        // WEAPON STUFF
        public event Action OnWeaponSwitch;
        public event Action OnWeaponShoot;
        
        private bool equippedBefore;
        private const float CharacterHeight = 1;
        private PhysicsMaterial pM;
        
        private float climbY;
        private float xAxis, yAxis;
        
        [HideInInspector]
        public Quaternion rotationAux, aimRotationSpineAux, aimRotationAux;

        [HideInInspector]
        public float lean;
        [HideInInspector]
        public float recoil;
        private float capsuleSize;
        [HideInInspector]
        public float currentMovementState;
        [HideInInspector]
        public bool isRunning;
        [HideInInspector]
        public float runKeyPressed;
        private AnimatorStateInfo currentAnimatorState;
        [HideInInspector]
        public bool grounded, inMoveState, isClimbing, climbHit, switchingWeapons, halfSwitchingWeapons;
        private Quaternion climbRotation;

        [HideInInspector]
        public WeaponBase currentWeapon;

        [HideInInspector]
        public int currentWeaponID;

        [HideInInspector]
        public bool equippedWeapon;

        [HideInInspector]
        public Transform leftHandInWeapon, rightHandInWeapon;
        
        [HideInInspector]
        public Quaternion startSpineRot = new Quaternion(0, 0, 0, 1);
        
        // Swimming
        [HideInInspector]
        public bool isSwimming;
        
        #endregion

        #region Animation Hashes
        private static readonly int Move = Animator.StringToHash("Move");
        private static readonly int Climb1 = Animator.StringToHash("Climb");
        private static readonly int AxisX = Animator.StringToHash("AxisX");
        private static readonly int AxisY = Animator.StringToHash("AxisY");
        private static readonly int Grounded = Animator.StringToHash("Grounded");
        private static readonly int Speed = Animator.StringToHash("Speed");
        private static readonly int HoldingWeapon = Animator.StringToHash("HoldingWeapon");
        private static readonly int State = Animator.StringToHash("State");
        private static readonly int JumpForward = Animator.StringToHash("Jump Forward");
        private static readonly int JumpStanding = Animator.StringToHash("Jump Standing");
        private static readonly int CanAttack = Animator.StringToHash("CanAttack");
        private static readonly int CanAttackCombo = Animator.StringToHash("CanAttackCombo");
        private static readonly int CanAttackFinish = Animator.StringToHash("CanAttackComboFinish");
        private static readonly int Swimming = Animator.StringToHash("Swimming");
        #endregion

        #region Mono Behaviours
        
        public void Start()
        {
            //Cursor.lockState = CursorLockMode.Locked;
            if (!Application.isPlaying) return;
            // startSpineRot = aimHelperSpine.rotation;
            // startSpineRot = aimHelper.rotation;
            //startSpineRot = new Quaternion(0.2f, -0.2f, 0, 1);
            rotationAux = new Quaternion(0, 0, 0, 1);
            aimRotationAux = rotationAux;
            aimRotationSpineAux = rotationAux;
            crouch = false;
            halfSwitchingWeapons = true;

            humanoidInventory = GetComponent<HumanoidInventory>();
            rb = playerAnimator.GetComponent<Rigidbody>();
            capsuleCollider = playerAnimator.GetComponent<CapsuleCollider>();
            pathMaker = playerAnimator.GetComponent<TransformPathMaker>();

            foreach (var r in boneRb)
            {
                var bc = r.GetComponent<BoxCollider>();
                var sc = r.GetComponents<SphereCollider>();
                if (bc != null) {
                    Physics.IgnoreCollision(capsuleCollider, bc);
                }

                if (sc == null) continue;
                foreach (var s in sc)
                {
                    Physics.IgnoreCollision(capsuleCollider, s);
                }
            }
            pM = capsuleCollider.material;
        }
        
        private void Update()
        {
            if (!Application.isPlaying) { return; }
            //if (Input.GetMouseButtonDown(0)) { Cursor.lockState = CursorLockMode.Locked; }

            // Ground Check
            GroundCheck();
            
            // Detect if we are grounded
            HumanoidSensors();

            // Animator State Controller
            AnimatorMovementState();
            
            // Check if we need to ragdoll
            RagdollWhenFall();
            
            // Track root bone when in ragdoll state
            TrackRagdollRoot();

            // Climb Logic
            //ClimbLogic();
            
            // Swim Logic
            SwimLogic();
            
            // Recoil
            recoil = Mathf.Lerp(recoil, 0, 10 * Time.deltaTime);

        }
        
        private void LateUpdate()
        {
            if (!Application.isPlaying) { return; }
            if (humanoidType == Type.Npc) return;
            if (humanoidStatus == Status.Dead) { return; }
        }
        
        /*
         * Only use for Physics based calculations
         */
        private void FixedUpdate()
        {
            if (humanoidStatus == Status.Dead) { return; }

            // Apply Gravity
            GravityPhysics();
            
            // Handle Player Movement
            PlayerMovement();
            
            // Handle Swim Physics
            SwimPhysics();
        }
        
        #endregion
        
        #region Public Api

        public void UseWeapon()
        {
            if (isClimbing || switchingWeapons || isSwimming || humanoidStatus == Status.Dead) return;

            if (!equippedWeapon)
            {
                // We can punch
                if (crouch) return;
                if (!playerFeatures.canMeleeAttack) return;
                if (!playerAnimator.GetBool(CanAttack))
                {
                    isAttacking = true;
                    playerAnimator.SetBool(CanAttack, true);
                }
                else if (playerAnimator.GetBool(CanAttack) && !playerAnimator.GetBool(CanAttackCombo) &&
                         !playerAnimator.GetBool(CanAttackFinish))
                {
                    isAttacking = true;
                    playerAnimator.SetBool(CanAttackCombo, true);
                }
                else if (playerAnimator.GetBool(CanAttack) && playerAnimator.GetBool(CanAttackCombo) &&
                         !playerAnimator.GetBool(CanAttackFinish))
                {
                    isAttacking = true;
                    playerAnimator.SetBool(CanAttackFinish, true);
                }
            }
            else
            {
                currentWeapon.Shoot();
                OnWeaponShoot?.Invoke();
            }
        }
        
        public void ToggleWeapon()
        {
            if (switchingWeapons || !(humanoidInventory.WeaponCount() > 0) || isClimbing || isSwimming) return;
            EquipWeaponToggle();
        }

        public void ReloadWeapon()
        {
            if (!equippedWeapon || switchingWeapons) return;
            currentWeapon.Reload();
            OnWeaponShoot?.Invoke();
        }

        public void PlayerRun(bool run)
        {
            isRunning = run;
        }
        
        public void ToggleRun()
        {
            isRunning = !isRunning;
        }

        public void SetXAxis(float x)
        {
            xAxis = x;
        }
        
        public void SetYAxis(float y)
        {
            yAxis = y;
        }

        void EquipWeaponToggle()
        {
            equippedWeapon = !equippedWeapon;

            if (humanoidInventory.WeaponCount() > 0)
            {
                if (equippedWeapon)
                {
                    currentWeapon = GetCurrentWeapon();
                    leftHandInWeapon = currentWeapon.leftHand;
                    rightHandInWeapon = currentWeapon.rightHand;
                }
                currentWeapon.ToggleRenderer(equippedWeapon);
            }

            OnWeaponSwitch?.Invoke();
        }

        public void SwitchWeapon(int numberPressed)
        {
            if (isSwimming) return;
            if (numberPressed >= humanoidInventory.WeaponCount() || GetCurrentWeapon().reloadProgress) return;
            if (numberPressed != currentWeaponID)
            {
                if (!switchingWeapons)
                {
                    StartCoroutine(WeaponSwitchProgress(numberPressed));
                }
            }
            if (!equippedWeapon)
            {
                EquipWeaponToggle();
            }
        }
        
        public void ToggleRagdoll()
        {
            if(boneRb[0].linearVelocity.magnitude > 1 && !isSwimming) { return; }
            var ragdoll = !ragdollHelper.ragdolled;
            
            foreach (var r in boneRb)
            {
                if (ragdoll == false) {
                    capsuleCollider.enabled = true;
                    ragdollHelper.ragdolled = false;
                    r.isKinematic = true;
                    r.linearVelocity = Vector3.zero;
                    boneRb[0].transform.parent = hipsParent;
                    // if (humanoidType == Type.Npc)
                    // {
                    //     NpcBehaviour npc = GetComponent<NpcBehaviour>();
                    //     npc.aiType = NpcBehaviour.AITarget.Waypoints;
                    // }
                    //cameraParent.parent = transform;
                }
                else 
                {
                    // if (humanoidType == Type.Npc)
                    // {
                    //     NpcBehaviour npc = GetComponent<NpcBehaviour>();
                    //     npc.aiType = NpcBehaviour.AITarget.Idle;
                    // }
                    if (equippedWeapon) EquipWeaponToggle();
                    crouch = false;
                    ragdollHelper.ragdolled = true;
                    isAiming = false;
                    pathMaker.Reset();
                    rb.useGravity = false;
                    r.isKinematic = false;
                    r.linearVelocity = rb.linearVelocity * 1.5f;
                    playerAnimator.SetFloat(Move, 0);
                    playerAnimator.enabled = false;
                    rb.linearVelocity = Vector3.zero;
                    rb.isKinematic = true;
                    capsuleCollider.enabled = false;
                    boneRb[0].transform.parent = null;
                    //cameraParent.parent = null;
                }
            }
        }

        public void RagdollWhenFall()
        {
            if (ragdollHelper.ragdolled || !playerFeatures.ragdollWhenFall) return;
            if (rb.linearVelocity.y < -15)
            {
                ToggleRagdoll();
            }
        }
        
        public bool SomethingInFront()
        {
            Vector3 posToDetect = transformToRotate.position + transformToRotate.up * .5f;
            return Physics.Raycast(posToDetect, transformToRotate.forward, 0.5f, frontLayers);
        }

        public void SetAim(bool newAim)
        {
            if (newAim == isAiming) return;
            if (isClimbing || !grounded || ragdollHelper.ragdolled || isSwimming) return;
            isAiming = newAim;
            if (equippedWeapon) currentWeapon.AimAudio();
        }
        
        public void ToggleAim()
        {
            if (equippedWeapon)
            {
                var currentW = currentWeapon;
                if (!currentW.reloadProgress && !currentW.shootProgress)
                {
                    SetAim(!isAiming);
                }
                else
                {
                    SetAim(true);
                }
            }
            else
            {
                SetAim(!isAiming);
            }
        }

        private void Aim()
        {
            if (!equippedWeapon || humanoidStatus == Status.Dead) return;
            currentWeapon.MoveTo(!isAiming ? transformToRotate : aimHelper);
            if (humanoidType == Type.Player) 
                aimHelper.rotation = aimRotationAux;
        }

        public void SetCrouch()
        {
            if (!playerFeatures.canCrouch) return;
            if (humanoidStatus == Status.Dead) return;
            if (isSwimming) return;
            var transformCache = playerAnimator.transform;
            var transformCacheUp = transformCache.up;
            var somethingAbove = Physics.Raycast(transformCache.position + transformCacheUp * .5f, transformCacheUp, 5.4f);

            if (crouch && !somethingAbove)
            {
                crouch = false;
            }
            else
            {
                crouch = true;
            }
        }

        public void SetStand()
        {
            if (humanoidStatus == Status.Dead) return;
            var transformCache = playerAnimator.transform;
            var transformCacheUp = transformCache.up;
            var somethingAbove = Physics.Raycast(transformCache.position + transformCacheUp * .5f, transformCacheUp, 1.4f);
            if (crouch && !somethingAbove)
            {
                crouch = false;
            }
            
            if (!ragdollHelper.ragdolled) return;

            if (Physics.SphereCast(transformCache.position + transformCacheUp * 1, 0.2f, -transformCacheUp, out _, 3f))
            {
                ToggleRagdoll();
            }
        }
        
        public void Jump()
        {
            if (!playerFeatures.canJump) return;
            var canJumpBasedOnWeapon = true;

            if (currentWeapon != null)
            {
                if (currentWeapon.reloadProgress)
                {
                    canJumpBasedOnWeapon = false;
                }
            }

            if (grounded /*&& inMoveState*/ && !isClimbing && !crouch && !climbHit && !isAiming && !ragdollHelper.ragdolled && canJumpBasedOnWeapon)
            {
                //if (moveAxis != Vector3.zero && !SomethingInFront())
                //{
                //    playerAnimator.SetTrigger(JumpForward);
                //    rb.linearVelocity = transformToRotate.up * playerSettings.jumpForce + transformToRotate.forward * 4;
                //}
                //else
                {
                    playerAnimator.SetTrigger(JumpStanding);
                    rb.linearVelocity = transformToRotate.up * playerSettings.jumpForce / 1.1f;
                }
            }
        }
        
        #endregion

        #region Private Functions
        
        private void PlayerMovement()
        {
            //if (humanoidStatus == Status.Dead) { return; }
            
            if (/*(inMoveState || isSwimming) &&*/ !isClimbing)
            {

                // SPEED CHANGE
                float speed = 0;
                if (currentMovementState < 0.5f)
                {
                    speed = playerSettings.crouchSpeed;
                }
                else if (currentMovementState < 1.5f)
                {
                    if (runKeyPressed > 1.5f && !crouch && !isAiming)
                    {
                        speed = isSwimming?advancedSettings.swimFastSpeed:playerSettings.runSpeed;
                    }
                    else
                    {
                        speed = isSwimming?advancedSettings.swimSpeed:playerSettings.walkSpeed;
                    }

                }
                else if (currentMovementState < 3.5f)
                {
                    speed = playerSettings.aimWalkSpeed;
                }

                // Handle movement based on move Axis
                if (!SomethingInFront())
                {
                    if (isRunning && moveAxis != Vector3.zero) { LerpSpeed(2); } else { LerpSpeed(1); }
                    
                    if (grounded || isSwimming)
                    {
                        var moveSpeed = moveAxis.normalized * speed;
                        if (!playerAnimator.hasRootMotion)
                        {
                            rb.linearVelocity = new Vector3(moveSpeed.x, rb.linearVelocity.y, moveSpeed.z);
                        }
                    }

                }
                else
                {
                    if (isAiming)
                    {
                        //if (grounded)
                        {
                            var moveSpeed = moveAxis.normalized * speed;
                            if (!playerAnimator.hasRootMotion)
                            {
                                rb.linearVelocity = new Vector3(moveSpeed.x, rb.linearVelocity.y, moveSpeed.z);
                            }
                        }

                    }
                }
                Crouch();
                Aim();
            }
            
        }
        
        private IEnumerator WeaponSwitchProgress(int numberP)
        {
            switchingWeapons = true;
            halfSwitchingWeapons = false;
            yield return new WaitForSeconds(advancedSettings.switchWeaponTime/2);
            halfSwitchingWeapons = true;
            if (currentWeapon)
            {
                currentWeapon.ToggleRenderer(false);
            }
            currentWeaponID = numberP;
            currentWeapon = GetCurrentWeapon();
            OnWeaponSwitch?.Invoke();
            if (equippedWeapon)
            {
                currentWeapon.ToggleRenderer(true);
                leftHandInWeapon = currentWeapon.leftHand;
                rightHandInWeapon = currentWeapon.rightHand;
            }
            yield return new WaitForSeconds(advancedSettings.switchWeaponTime);
            switchingWeapons = false;
        }

        private void AnimatorMovementState()
        {
            currentAnimatorState = playerAnimator.GetCurrentAnimatorStateInfo(0);
            inMoveState = currentAnimatorState.IsName("Grounded");
            playerAnimator.SetBool(Grounded, grounded);
            playerAnimator.SetFloat(Speed, runKeyPressed);
            playerAnimator.SetBool(HoldingWeapon, equippedWeapon);
            playerAnimator.SetBool(Swimming, isSwimming);
            currentMovementState = playerAnimator.GetFloat(State);

            if (crouch)
            {
                playerAnimator.SetFloat(State, Mathf.Lerp(currentMovementState, 0, 5 * Time.deltaTime));
            }
            else if (isAiming)
            {
                if (!equippedWeapon)
                {
                    if (playerFeatures.canMeleeAttack)
                    {
                        //playerAnimator.SetLayerWeight(1,Mathf.Lerp(boxingLayerWeight, 1, 5 * Time.deltaTime));
                        playerAnimator.SetFloat(State, Mathf.Lerp(currentMovementState, 3, 5 * Time.deltaTime));
                    }
                    else
                    {
                        playerAnimator.SetFloat(State, Mathf.Lerp(currentMovementState, 2, 5 * Time.deltaTime));
                    }
                }
                else
                {
                    //playerAnimator.SetLayerWeight(1,Mathf.Lerp(boxingLayerWeight, 0, 5 * Time.deltaTime));
                    playerAnimator.SetFloat(State, Mathf.Lerp(currentMovementState, 2, 5 * Time.deltaTime));
                }
            }
            else
            {
                //playerAnimator.SetLayerWeight(1,Mathf.Lerp(boxingLayerWeight, 0, 5 * Time.deltaTime));
                playerAnimator.SetFloat(State, Mathf.Lerp(currentMovementState, 1, 5 * Time.deltaTime));
            }
            if (!SomethingInFront())
            {
                var m = Mathf.Clamp01(Mathf.Abs(xAxis) + Mathf.Abs(yAxis));
                playerAnimator.SetFloat(Move, Mathf.Lerp(playerAnimator.GetFloat(Move), m * runKeyPressed, 10 * Time.deltaTime));
                playerAnimator.SetFloat(AxisX, Mathf.Lerp(playerAnimator.GetFloat(AxisX), xAxis, 10 * Time.deltaTime));
                playerAnimator.SetFloat(AxisY, Mathf.Lerp(playerAnimator.GetFloat(AxisY), yAxis, 10 * Time.deltaTime));
            }
            else
            {
                if (isAiming)
                {

                    yAxis = Mathf.Clamp(yAxis, -1, 0);

                    var m = Mathf.Clamp01(Mathf.Abs(xAxis) + Mathf.Abs(yAxis));

                    playerAnimator.SetFloat(Move, Mathf.Lerp(playerAnimator.GetFloat(Move), m * runKeyPressed, 10 * Time.deltaTime));
                    playerAnimator.SetFloat(AxisX, Mathf.Lerp(playerAnimator.GetFloat(AxisX), xAxis, 10 * Time.deltaTime));
                    playerAnimator.SetFloat(AxisY, Mathf.Lerp(playerAnimator.GetFloat(AxisY), yAxis, 10 * Time.deltaTime));
                }
                else
                {
                    playerAnimator.SetFloat(Move, Mathf.Lerp(playerAnimator.GetFloat(Move), 0, 5 * Time.deltaTime));
                    playerAnimator.SetFloat(AxisX, Mathf.Lerp(playerAnimator.GetFloat(AxisX), 0, 10 * Time.deltaTime));
                    playerAnimator.SetFloat(AxisY, Mathf.Lerp(playerAnimator.GetFloat(AxisY), 0, 10 * Time.deltaTime));
                }
            }
        }
        
        private void Crouch()
        {
            if (!playerFeatures.canCrouch) return;
            if (humanoidStatus == Status.Dead) return;
            if (isSwimming) return;
            var transformCache = transform;
            var transformCacheUp = transformCache.up;
            var somethingAbove = Physics.Raycast(transformCache.position + transformCacheUp * .5f, transformCacheUp, 1.4f);
            if (somethingAbove)
            {
                crouch = true;
            }

            capsuleSize = Mathf.Lerp(capsuleSize, crouch ? playerSettings.crouchHeight : CharacterHeight, 5 * Time.deltaTime);
            capsuleCollider.center = new Vector3(0, .9f * capsuleSize, 0);
            capsuleCollider.height = 1.8f * capsuleSize;
        }
        
        private void ClimbLogic()
        {
            if (crouch) return;
            var canClimbBasedOnWeapon = true;

            if (currentWeapon != null)
            {
                if (currentWeapon.reloadProgress)
                {
                    canClimbBasedOnWeapon = false;
                }
            }

            if (hasFrontSensorHit && !ragdollHelper.ragdolled && canClimbBasedOnWeapon)
            {
                if (hasUpwardsSensorHit && upwardsSensorHit.transform.gameObject.layer != LayerMask.GetMask("Water"))
                    return;

                var hit = frontSensorHit;
                climbHit = true;
                climbY = hit.point.y;
                var dist = climbY - playerAnimator.transform.position.y;

                if (hit.collider.CompareTag("Climbable"))
                {

                    var right = transformToRotate.right;
                    var forward = transformToRotate.forward;
                    ikControl.leftHandPos = hit.point + right * -0.3f + forward * -0.3f;
                    ikControl.rightHandPos = hit.point + right * 0.3f + forward * -0.3f;
                    
                    if (pathMaker.play == false)
                    {
                        // var rotationLookAt = Quaternion.LookRotation(playerAnimator.transform.position, hit.point);
                        //playerAnimator.transform.rotation = Quaternion.RotateTowards(playerAnimator.transform.rotation, rotationLookAt, Time.deltaTime * 1f);
                        // playerAnimator.transform.rotation = Quaternion.FromToRotation(-Vector3.forward, hit.normal);
                        // playerAnimator.transform.rotation = Quaternion.LookRotation(-hit.normal);

                        equippedBefore = equippedWeapon;
                        if (equippedWeapon)
                        {
                            EquipWeaponToggle();
                        }

                        if (dist > 1f && dist < 1.8f)
                        {
                            isClimbing = true;
                            isSwimming = false;
                            isAiming = false;
                            
                            if (hasOutwardSensorHit)
                            {
                                climbRotation = Quaternion.LookRotation(-outwardSensorHit.normal);
                                playerAnimator.transform.rotation = climbRotation;
                            }
                            
                            playerAnimator.SetTrigger(Climb1);

                            pathMaker.pointsTime[0] = Vector3.Distance(playerAnimator.transform.position, pathMaker.points[0]);
                            pathMaker.points[0].y = climbY - 1.5f;

                            pathMaker.pointsTime[1] = 1;
                            pathMaker.points[1].y = climbY + 0.8f;
                            pathMaker.points[1].z = 1f;

                            pathMaker.pointsTime[2] = 1;
                            pathMaker.points[2].y = climbY + 1.3f;
                            pathMaker.points[2].z = 1f;
                            pathMaker.Play();
                            return;
                        }
                    }
                        
                }
                if (isClimbing)
                {

                }
            }
            else
            {
                climbHit = false;
                isClimbing = false;
                if (equippedBefore)
                {
                    equippedBefore = false;
                    EquipWeaponToggle();
                }
            }
        }

        private void SwimLogic()
        {
            
            // Trigger Swim State
            if (hasDetectableHit && detectableHit.transform.gameObject.layer == LayerMask.NameToLayer("Water"))
            {
                advancedSettings.swimWaterLevel = detectableHit.transform.position.y;
                if (detectableHit.distance < 3.75f)
                {
                    if (!isSwimming)
                    {
                        if (equippedWeapon) EquipWeaponToggle();
                        if (rb.linearVelocity.y < -6) audioSource.PlayOneShot(advancedSettings.splashAudio);
                        if (crouch) crouch = false;
                        if (isAiming) isAiming = false;
                        isSwimming = true;

                        // If we are in ragdoll we need to play the splash and toggle ragdoll
                        if (ragdollHelper.ragdolled)
                        {
                            audioSource.PlayOneShot(advancedSettings.splashAudio);
                            ToggleRagdoll();
                        }
                        
                        // Run Event
                        OnSwimEnter?.Invoke();
                    }
                }
                else
                {
                    if (isSwimming)
                    {
                        isSwimming = false;
                        
                        // Run Event
                        OnSwimExit?.Invoke();
                    }
                }
            }

            // Handle Swim Audio
            SwimAudio();

            // playerFeatures.useGravity = !isSwimming;
        }

        private void SwimAudio()
        {
            if (!isSwimming)
            {
                if (!audioSource.isPlaying) return;
                audioSource.loop = false;
                // audioSource.Stop();
                return;
            }
            
            if (moveAxis != Vector3.zero)
            {
                audioSource.pitch = isRunning ? 1.15f : 0.90f;
                audioSource.loop = true;
                if (audioSource.isPlaying) return;
                audioSource.clip = advancedSettings.swimAudio;
                audioSource.Play();
            }
            else
            {
                audioSource.loop = false;
            }
        }

        private void SwimPhysics()
        {
            if (!isSwimming) return;
            var actionPoint = playerAnimator.transform.position + playerAnimator.transform.TransformDirection(advancedSettings.swimBuoyancyCentreOffset);
            var forceFactor = 1f - (actionPoint.y - advancedSettings.swimWaterLevel) / advancedSettings.swimFloatHeight;
            if (!(forceFactor > 0f)) return;
            var uplift = -Physics.gravity * (forceFactor - rb.linearVelocity.y * advancedSettings.swimBounceDamp);
            rb.AddForceAtPosition(uplift, actionPoint);
        }

        private void HumanoidSensors()
        {

            var baseTransform = playerAnimator.transform;
            var baseTransformUp = baseTransform.up;
            var baseTransformForward = baseTransform.forward;
            var baseSensorPosition = baseTransform.position;
            
            // Upwards Sensor
            var upwardsSensorStart = baseSensorPosition;
            upwardsSensorStart.y += 1f;
            Debug.DrawLine(upwardsSensorStart, upwardsSensorStart + Vector3.up * 2f, Color.yellow);
            hasUpwardsSensorHit = Physics.Raycast(upwardsSensorStart, Vector3.up, out upwardsSensorHit, 2f, upwardsSensorLayerMask);
            
            // Front Sensor (Climb Sensor)
            var frontSensorStart = baseSensorPosition + transformToRotate.forward * 0.45f + transformToRotate.up * (2.1f * CharacterHeight);
            Debug.DrawLine(frontSensorStart, frontSensorStart + -baseTransformUp, Color.blue);
            hasFrontSensorHit = Physics.Raycast(frontSensorStart, -baseTransformUp, out frontSensorHit, 1.8f);
            
            // Outwards Sensor Z-Forward
            var outwardsSensorStart = baseSensorPosition;
            outwardsSensorStart.y += 1f;
            Debug.DrawLine(outwardsSensorStart, outwardsSensorStart + baseTransformForward * 1.8f, Color.magenta);
            hasOutwardSensorHit = Physics.Raycast(outwardsSensorStart, baseTransformForward, out outwardSensorHit, 1.8f);

            // Downwards Sensor
            var downwardSensorStart = baseSensorPosition;
            downwardSensorStart.y += heightFromGroundRaycast;
            Debug.DrawLine(downwardSensorStart, downwardSensorStart + Vector3.down * (raycastDownDistance +  heightFromGroundRaycast), Color.green);
            hasDetectableHit = Physics.Raycast(downwardSensorStart, Vector3.down, out detectableHit, raycastDownDistance + heightFromGroundRaycast,detectableLayers);
        }

        private void TrackRagdollRoot()
        {
            if (!ragdollHelper.ragdolled) return;
            if (boneRb[0].transform.parent == null)
            {
                playerAnimator.transform.position = boneRb[0].position;
            }

            // if (!isSwimming) return;
            // if (Physics.SphereCast(playerAnimator.transform.position + playerAnimator.transform.up * 1, 0.2f, -playerAnimator.transform.up, out _,3f, LayerMask.GetMask("Water")))
            // {
            //     ToggleRagdoll();
            // }
        }

        private WeaponBase GetCurrentWeapon()
        {
            return humanoidInventory.WeaponCount() > 0 ? humanoidInventory.weapons[currentWeaponID] : null;
        }
        
        #endregion

        #region Utility Functions

        private void GroundCheck()
        {
            if(Physics.SphereCast(playerAnimator.transform.position + playerAnimator.transform.up * 2, .15f, -playerAnimator.transform.up, out _, 2.5f, groundLayers))
            {
                grounded = true;
                if (moveAxis == Vector3.zero || ragdollHelper.state == RagdollHelper.RagdollState.blendToAnim)
                {
                    pM.staticFriction = 3;
                    pM.dynamicFriction = 3;
                }
                else
                {
                    pM.staticFriction = 0;
                    pM.dynamicFriction = 0;
                }
            }
            else
            {
                grounded = false;
                pM.staticFriction = 0;
                pM.dynamicFriction = 0;
            }
        }

        private void LerpSpeed(float final)
        {
            runKeyPressed = Mathf.Lerp(runKeyPressed, final, 10 * Time.deltaTime);
        }

        private void GravityPhysics()
        {
            if (!playerFeatures.useGravity || isSwimming) return;
            if (ragdollHelper.state != RagdollHelper.RagdollState.animated) return;
            var velocity = rb.linearVelocity;
            velocity.y += playerSettings.baseGravity * Time.deltaTime;
            rb.linearVelocity = velocity;
        }
        
        #endregion
        
    }
}