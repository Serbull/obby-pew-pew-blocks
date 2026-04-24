using UnityEngine;

public class PlayerAimRotation : MonoBehaviour
{
	public Camera playerCamera;
	public Animator animator;
	public float rotateSpeed = 15f;

	void LateUpdate()
	{
		if (animator == null) return;
		if (!animator.GetBool("IsAiming")) return;

		RotatePlayer();
	}

	void RotatePlayer()
	{
		Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
		Plane plane = new Plane(Vector3.up, transform.position);

		if (plane.Raycast(ray, out float dist))
		{
			Vector3 hit = ray.GetPoint(dist);

			Vector3 dir = hit - transform.position;
			dir.y = 0f;

			if (dir.sqrMagnitude < 0.001f) return;

			Quaternion rot = Quaternion.LookRotation(dir);

			transform.rotation = Quaternion.Slerp(
				transform.rotation,
				rot,
				rotateSpeed * Time.deltaTime
			);
		}
	}
}