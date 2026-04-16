using Cinemachine;
using UnityEngine;

public class FollowCameraController : Singleton<FollowCameraController>
{
    [SerializeField] private Transform _target;
    [SerializeField] private float _cameraOffsetY = 1.5f;
    [SerializeField] private float _cameraOffsetZMin = 4f;
    [SerializeField] private float _cameraOffsetZMax = 6f;
    [SerializeField] private float _zoomSensitivity = 1f;
    [Space]
    [SerializeField] private LayerMask _collisionLayers;
    [Space]
    [SerializeField] private Vector2 _minMaxFov;

    private Transform _cameraTransform;
    private Transform _cameraTarget;
    private Vector3 _cameraEuler;
    private Vector3 _cameraOffset;

    private float _cameraOffsetZ;
    private float _offsetScale = 1;
    private float _collisionOffsetDistance;
    private Vector3 _targetPrevPosition;
    private float _fov;

    private CinemachineVirtualCamera _camera;
    private CinemachineTransposer _transposer;

    public Transform CameraTransform => _cameraTransform;

    private void Start()
    {
        _cameraTarget = new GameObject("CameraTarget").transform;
        SetTarget(_target);
        _cameraOffsetZ = _cameraOffsetZMin + (_cameraOffsetZMax - _cameraOffsetZMin) * 0.2f;
        _targetPrevPosition = _cameraTarget.position ;
        _fov = _minMaxFov.x;
        _camera = CameraRig.Instance.MainCamera;
        _transposer = _camera.GetCinemachineComponent<CinemachineTransposer>();
        _cameraTransform = _camera.transform;
        _camera.Follow = _cameraTarget;
        _camera.LookAt = _cameraTarget;
        Rotate(90, -15);
    }

    private void FixedUpdate()
    {
        CheckCollision();

        _cameraOffset = Quaternion.Euler(_cameraEuler) * new Vector3(_cameraOffsetZ - _collisionOffsetDistance, 0, 0);

        _transposer.m_FollowOffset = _cameraOffset;

        CalculateFov();
    }

    private void CheckCollision()
    {
        Vector3 direction = _cameraTransform.position - _cameraTarget.position;
        var maxDistance = _cameraOffsetZ - Vector3.Distance(_cameraTarget.position, _cameraTarget.position);

        if (Physics.SphereCast(_cameraTarget.position, 0.2f, direction, out RaycastHit hit, maxDistance, _collisionLayers, QueryTriggerInteraction.Ignore))
        {
            _collisionOffsetDistance = _cameraOffsetZ - Vector3.Distance(_cameraTarget.position, hit.point);
        }
        else
        {
            _collisionOffsetDistance = 0;
        }
    }

    private void CalculateFov()
    {
        var speed = Vector3.Distance(_cameraTarget.position, _targetPrevPosition) / Time.fixedDeltaTime;
        var targetFov = Mathf.Lerp(_minMaxFov.x, _minMaxFov.y, speed / 1000);
        _fov = Mathf.Lerp(_fov, targetFov, 5 * Time.fixedDeltaTime);
        _camera.m_Lens.FieldOfView = _fov;
        _targetPrevPosition = _cameraTarget.position;
    }

    public void Rotate(float x, float y)
    {
        _cameraEuler.y += x;
        _cameraEuler.z = Mathf.Clamp(_cameraEuler.z - y, -30f, 80f);
    }

    public void Zoom(float value)
    {
        _cameraOffsetZ = Mathf.Clamp(_cameraOffsetZ - value * _zoomSensitivity, _cameraOffsetZMin * _offsetScale, _cameraOffsetZMax * _offsetScale);
    }

    public void SetTarget(Transform transform)
    {
        _target = transform;
        _cameraTarget.SetParent(_target);
        _cameraTarget.localPosition = Vector3.up * _cameraOffsetY;
    }

    public void SetOffsetScale(float scale)
    {
        _offsetScale = scale;
        Zoom(0);
    }
}
