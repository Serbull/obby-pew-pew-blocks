using UnityEngine;

public class LookAtCameraAlways : MonoBehaviour
{
    private Transform _mainCamera;

    private void Start()
    {
        _mainCamera = Camera.main.transform;   
    }

    private void LateUpdate()
    {
        transform.rotation = _mainCamera.rotation;
    }
}
