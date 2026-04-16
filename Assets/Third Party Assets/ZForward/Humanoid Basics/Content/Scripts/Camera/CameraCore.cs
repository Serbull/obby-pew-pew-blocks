/*
 * CameraCore.cs - ZForward
 * @version: 1.1.0
*/

using Humanoid_Basics.Core.Helpers;
using UnityEngine;
using Cinemachine;

namespace Humanoid_Basics.Camera
{
    public class CameraCore : Singleton<CameraCore>
    {
        public const string Version = "0.1.0";

        // The main camera
        [HideInInspector]
        public GameObject cameraObject;

        // The target of the camera.
        public Transform target;

        // The offsets from the target of the camera.
        public Vector2 targetOffset = new Vector2(0.5f, 1.5f);
        private Vector2 targetOffsetModifer = new Vector2(0f, 0);
        private Vector2 currentOffset;

        // The distance from the target.
        public float targetDistance = 2;

        //
        public bool smoothCamera;
        public float smoothCameraRate = 3f;

        [SerializeField] public LayerMask collisionLayers;

        // The target modifiers
        [HideInInspector]
        public float targetDistanceModifier;
        [HideInInspector]
        public Vector3 targetPositionModifier;
        [HideInInspector]
        public Quaternion targetRotationModifier;

        // Reference to the camera pivot transforms.
        [HideInInspector]
        public Transform[] cameraPivot = new Transform[2];

        // Rotate the target to camera forward
        public bool useTargetOffset;

        // Private Vars
        private float currentCamDistance, cameraXAxis, cameraZAxis, cameraZClamp;

       [SerializeField] private CinemachineVirtualCamera _virtualCamera;

        private void Start()
        {
            // Find the camera attached to this GameObject
            currentCamDistance = targetDistance;
        }

        //private void Update()
        //{
        //    if(Input.GetKeyDown(KeyCode.S))
        //    {
        //        staticUpdate = !staticUpdate;
        //    }
        //}

        //private void FixedUpdate()
        //{
        //    if(staticUpdate)
        //    {
        //        transform.position = target.position + transform.rotation * (transform.forward + new Vector3(targetOffset.x, targetOffset.y, 0));
        //        return;
        //    }

        //    // Camera Parent Transform
        //    var cameraParentTransform = transform;
        //    var cameraTransform = cameraObject.transform;
        //    var cameraTransformForward = cameraTransform.forward;
        //    var cameraTransformRight = cameraTransform.right;

        //    // var currentOffset = targetOffset + targetOffsetModifer;
        //    currentOffset = Vector2.Lerp(currentOffset, targetOffset + targetOffsetModifer, smoothCameraRate * Time.fixedDeltaTime);


        //    // X Axis
        //    cameraPivot[0].localEulerAngles = new Vector3(0, cameraXAxis, 0);

        //    // Z Axis
        //    cameraZClamp = Mathf.Lerp(cameraZClamp, 70, 8 * Time.fixedDeltaTime);
        //    cameraZAxis = Mathf.Clamp(cameraZAxis, -60, cameraZClamp);
        //    cameraPivot[1].localEulerAngles = new Vector3(cameraZAxis, 0, 0);

        //    // Collision detection
        //    var startPoint = cameraPivot[0].position;
        //    if (Physics.SphereCast(startPoint, 0.1f, -cameraTransformForward, out var h, targetDistance / 2, collisionLayers) ||
        //        Physics.SphereCast(startPoint, 0.2f, -cameraTransformForward + cameraTransformRight * (currentOffset.x / 2), out h, targetDistance, collisionLayers))
        //    {
        //        var dist = Vector3.Distance(cameraPivot[0].position, h.point) - targetDistanceModifier;
        //        currentCamDistance = Mathf.Clamp(dist, 0.1f, targetDistance);
        //        cameraTransform.localPosition = Vector3.Lerp(cameraTransform.localPosition, new Vector3(0, 0, -currentCamDistance + 0.3f), 100f * Time.fixedDeltaTime);
        //    }
        //    else
        //    {
        //        currentCamDistance = targetDistance - targetDistanceModifier;
        //        cameraTransform.localPosition = Vector3.Lerp(cameraTransform.localPosition, new Vector3(useTargetOffset ? currentOffset.x : 0, 0, -currentCamDistance), 10f * Time.fixedDeltaTime);
        //    }

        //    // Lock the camera position onto the target
        //    var smoothPosition = target.transform.position + target.root.transform.up * (useTargetOffset ? currentOffset.y : 0);
        //    if (smoothCamera)
        //    {
        //        smoothPosition = Vector3.Lerp(cameraParentTransform.position, smoothPosition, smoothCameraRate * Time.fixedDeltaTime);
        //        // var velocity = Vector3.zero;
        //        // smoothPosition = Vector3.SmoothDamp(cameraParentTransform.position, smoothPosition, ref velocity, smoothCameraRate * Time.deltaTime);
        //    }
        //    cameraParentTransform.position = smoothPosition;

        //    // Adjust for rotation modifier
        //    cameraTransform.localRotation = Quaternion.Lerp(cameraTransform.localRotation, targetRotationModifier, 2f * Time.fixedDeltaTime);
        //}

        public void SetTarget(Transform targetTransform)
        {
            target = targetTransform;
            if(_virtualCamera == null)
            {
                Start();
            }
            _virtualCamera.Follow = target;
            _virtualCamera.LookAt = target;
        }

        public Transform GetTarget()
        {
            return target;
        }

        public void SetCameraX(float xAxis)
        {
            cameraXAxis += xAxis;
        }

        public void SetCameraZ(float zAxis)
        {
            cameraZAxis += zAxis;
        }

        public void SetTargetDistanceModifier(float modifier)
        {
            targetDistanceModifier = modifier;
        }

        public void SetTargetOffsetModifier(Vector2 modifier)
        {
            targetOffsetModifer = modifier;
        }


        public void SetCameraXZ(float xAxis, float zAxis)
        {
            cameraXAxis = xAxis;
            cameraZAxis = zAxis;
        }
    }
}
