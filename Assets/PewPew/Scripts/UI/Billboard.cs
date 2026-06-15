using UnityEngine;

public class Billboard : MonoBehaviour
{
	private Transform mainCameraTransform;

	void Start()
	{
		// Находим главную камеру один раз при старте
		if (Camera.main != null)
		{
			mainCameraTransform = Camera.main.transform;
		}
	}

	void LateUpdate()
	{
		if (mainCameraTransform != null)
		{
			// С большой буквы 'L' — transform.LookAt
			transform.LookAt(transform.position + mainCameraTransform.rotation * Vector3.forward,
				mainCameraTransform.rotation * Vector3.up);
		}
	}
}