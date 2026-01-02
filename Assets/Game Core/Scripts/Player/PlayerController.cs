using UnityEngine;

public class PlayerController : Singleton<PlayerController>
{
    public enum State
    {
        normal,
        lockMovement
    }

    public CharacterCore CharacterCore;
    public FP_Input playerInput;
    public FollowCameraController _camera;

    [SerializeField] private float mouseSensitivity = 30;

    public State state = State.normal;

    [Header("Controls")]
    [SerializeField] private KeyCode jumpKey = KeyCode.Space;

    private Vector2 movementAxis;

    private void Update()
    {
        if (playerInput.UseMobileInput)
        {
            var input = playerInput.MoveInput();
            input.Normalize();
            input.x = playerInput.MovePressed() ? input.x : 0;
            input.z = playerInput.MovePressed() ? input.z : 0;

            movementAxis.x = input.x;
            movementAxis.y = input.z;

            var sensitivity = SaveManager.Data.cameraSensitivity * mouseSensitivity;
            _camera.Rotate(playerInput.LookInput().x * sensitivity, playerInput.LookInput().y * sensitivity);

            var zoom = ((playerInput.ZoomIn() ? 1 : 0) - (playerInput.ZoomOut() ? 1 : 0)) * Time.unscaledDeltaTime * 100f;
            if (zoom != 0)
            {
                _camera.Zoom(zoom);
            }
        }
        else
        {
            movementAxis.x = Input.GetAxisRaw("Horizontal");
            movementAxis.y = Input.GetAxisRaw("Vertical");

            if (Input.GetMouseButton(1))
            {
                var sensitivity = SaveManager.Data.cameraSensitivity * mouseSensitivity;
                _camera.Rotate(Input.GetAxis("Mouse X") * sensitivity, Input.GetAxis("Mouse Y") * sensitivity);
            }

            var zoom = Input.mouseScrollDelta.y;
            if (zoom != 0)
            {
                _camera.Zoom(zoom);
            }
        }

        HandleKeyPress();
    }

    private void FixedUpdate()
    {
        PlayerMovement();
    }

    private void PlayerMovement()
    {
        if (state == State.normal)
        {
            var cameraObjectTransform = _camera.CameraTransform;
            var cameraObjectTransformForward = cameraObjectTransform.forward;

            // As this is camera related i think this needs to be moved...
            var orientedX = movementAxis.x * cameraObjectTransform.right;
            var orientedY = movementAxis.y * cameraObjectTransformForward;

            orientedX.y = 0;
            orientedY.y = 0;

            CharacterCore.moveAxis = orientedY + orientedX;

            // Lets rotate the player animator to the new position if aiming.
            CharacterCore.transform.rotation = Quaternion.Lerp(CharacterCore.transform.rotation, CharacterCore.rotationAux, 10f * Time.fixedDeltaTime);

            // If we are moving and not aiming then calculate rotation for the next frame...
            if (CharacterCore.moveAxis != Vector3.zero)
            {
                CharacterCore.rotationAux = Quaternion.LookRotation((CharacterCore.transform.position + CharacterCore.moveAxis) - CharacterCore.transform.position);
            }
        }
    }

    private void HandleKeyPress()
    {
        if (playerInput.UseMobileInput ? playerInput.Jump() : Input.GetKey(jumpKey))
        {
            CharacterCore.Jump();
        }
    }
}
