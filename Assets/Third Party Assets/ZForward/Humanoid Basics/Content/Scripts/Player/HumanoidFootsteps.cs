/*
 * HumanoidFootsteps.cs - ZForward
 * @version: 1.0.0
*/

using System;
using Humanoid_Basics.Core;
using UnityEngine;

namespace Humanoid_Basics.Player
{
    [RequireComponent(typeof(HumanoidCore))]
    public class HumanoidFootsteps : MonoBehaviour
    {
        public const string Version = "1.0.0";
        
        public HumanoidCore humanoidCore;

        [Serializable]
        public sealed class AudioClips
        {
            public AudioClip defaultAudioClip;
            public AudioClip dirtAudioClip;
            public AudioClip woodAudioClip;
            public AudioClip grassAudioClip;
            public AudioClip stoneAudioClip;
            public AudioClip metalAudioClip;
            public AudioClip snowAudioClip;
            public AudioClip waterAudioClip;
        }
        
        [Header("Audio")]
        public AudioClips audioClips = new AudioClips();
        
        public Transform leftFoot, rightFoot;
        private bool leftCanStep;

        // [HideInInspector]
        public string groundType = "Default";
        public float distance;
        public float factor = 0.65f;
        public float hitDistance = 100000f;

        // Start is called before the first frame update
        private void Start()
        {
            humanoidCore = GetComponent<HumanoidCore>();
        }

        // Update is called once per frame
        private void Update()
        {
            FootStepAudio();
        }

        private void FootStepAudio()
        {
            if (!humanoidCore.grounded) { return; }
            if (humanoidCore.isClimbing || humanoidCore.isSwimming) { return; }
            distance = Vector3.Distance(leftFoot.position, rightFoot.position);
            if(distance > factor) leftCanStep = true;
            if (!leftCanStep || !(distance < factor)) return;
            leftCanStep = false;
            
            // TODO: Detect floor type (Grass, Dirt, Metal etc)
            var transform1 = transform;
            Vector3 rayPosition = transform1.position;
            rayPosition.y = rayPosition.y + 0.5f;
            var audioClip = audioClips.defaultAudioClip;
            groundType = "Default";
            
            if (humanoidCore.hasDetectableHit)
            {
                var hit = humanoidCore.detectableHit;
                Debug.Log("Hit: "+hit.transform.gameObject.layer);

                if (hit.transform.gameObject.layer == LayerMask.NameToLayer("Water") || hit.transform.gameObject.layer == LayerMask.NameToLayer("Ground"))
                {
                    var floorType = hit.transform.gameObject.GetComponent<FloorType>().type;
                    Debug.Log("Floor Type Confirmed: "+floorType.ToString());
                    switch (floorType)
                    {
                        case FloorType.Types.Dirt:
                            audioClip = audioClips.dirtAudioClip;
                            groundType = "Dirt";
                            break;
                        case FloorType.Types.Grass:
                            audioClip = audioClips.grassAudioClip;
                            groundType = "Grass";
                            break;
                        case FloorType.Types.Wood:
                            audioClip = audioClips.woodAudioClip;
                            groundType = "Wood";
                            break;
                        case FloorType.Types.Metal:
                            audioClip = audioClips.metalAudioClip;
                            groundType = "Metal";
                            break;
                        case FloorType.Types.Snow:
                            audioClip = audioClips.snowAudioClip;
                            groundType = "Snow";
                            break;
                        case FloorType.Types.Stone:
                            audioClip = audioClips.stoneAudioClip;
                            groundType = "Stone";
                            break;
                        case FloorType.Types.Water:
                            audioClip = audioClips.waterAudioClip;
                            groundType = "Water";
                            break;
                        case FloorType.Types.Default:
                            audioClip = audioClips.defaultAudioClip;
                            groundType = "Default";
                            break;

                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }
            
            Debug.Log("Played Footstep "+groundType);
            humanoidCore.audioSource.PlayOneShot(audioClip);
        }
    }
}
