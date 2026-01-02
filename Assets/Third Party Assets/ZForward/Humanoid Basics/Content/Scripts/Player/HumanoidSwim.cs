using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Humanoid_Basics.Player
{
    
    public class HumanoidSwim : MonoBehaviour
    {
        private HumanoidCore humanoidCore;
        
        [SerializeField]
        public float normalSpeed = 2.3f, fastSpeed = 2.3f;

        private float currentHeight;
        public float swimHeight = 0.5f;
        private float normalHeight;
        
        public float waterLevel, floatHeight;
        public Vector3 buoyancyCentreOffset;
        public float bounceDamp;

        public bool isSwimming = false;

        private static readonly int Swimming = Animator.StringToHash("Swimming");
        
        [Range(0, 5)] [SerializeField] private float heightFromGroundRaycast = 5f;
        [Range(0, 2)] [SerializeField] private float raycastDownDistance = 0.35f;
        [SerializeField] public LayerMask waterLayer;
        [SerializeField] public LayerMask waterLayerMasker;

        private void Start()
        {
            humanoidCore = GetComponent<HumanoidCore>();
            normalHeight = humanoidCore.capsuleCollider.height;
        }
        
        private void Update()
        {
            if (!isSwimming) return;
            
            var moveSpeed = humanoidCore.moveAxis.normalized * (humanoidCore.isRunning?fastSpeed:normalSpeed);
            humanoidCore.rb.linearVelocity = new Vector3(moveSpeed.x, humanoidCore.rb.linearVelocity.y, moveSpeed.z);
        }

        private void FixedUpdate()
        {
            humanoidCore.capsuleCollider.height = Mathf.Lerp(humanoidCore.capsuleCollider.height, currentHeight, 5 * Time.deltaTime);
            
            if (!CanSwim()) return;

            var actionPoint = humanoidCore.playerAnimator.transform.position + humanoidCore.playerAnimator.transform.TransformDirection(buoyancyCentreOffset);
            //Debug.Log(actionPoint);
            var forceFactor = 1f - (actionPoint.y - waterLevel) / floatHeight;

            if (!(forceFactor > 0f)) return;
            var uplift = -Physics.gravity * (forceFactor - humanoidCore.rb.linearVelocity.y * bounceDamp);
            humanoidCore.rb.AddForceAtPosition(uplift, actionPoint);
        }

        private bool CanSwim()
        {
            RaycastHit oHit;
            var start = humanoidCore.playerAnimator.transform.position;
            start.y += heightFromGroundRaycast;
            
            isSwimming = false;
            currentHeight = normalHeight;
            
            Debug.DrawLine(start, start + Vector3.down * (raycastDownDistance +  heightFromGroundRaycast), Color.red);
            if (Physics.Raycast(start, Vector3.down, out oHit, raycastDownDistance +  heightFromGroundRaycast, waterLayerMasker))
            {
                if (oHit.transform.gameObject.layer == LayerMask.NameToLayer("Water"))
                {
                    //Debug.Log("We are in Water");
                    if (oHit.distance < 3.75f)
                    {
                        currentHeight = swimHeight;
                        isSwimming = true;
                    }

                }
                
            }

            humanoidCore.playerFeatures.useGravity = !isSwimming;
            humanoidCore.playerAnimator.SetBool(Swimming, isSwimming);
            return isSwimming;
        }
    }
}
