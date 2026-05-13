using UnityEngine;

public class PlayerAimRotation : MonoBehaviour
{
	public Camera playerCamera;
	public Animator animator;
	public CharacterCore characterCore;

	public float rotateSpeed = 15f;

	void Update()
	{
		if (animator == null || characterCore == null) return;
		if (!animator.GetBool("IsAiming") || animator.GetFloat("Move") > 0) return;

		RotatePlayer();
	}

	void RotatePlayer()
	{
		Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
		RaycastHit hit;

		Vector3 targetPoint;

		if (Physics.Raycast(ray, out hit, 500f))
		{
			targetPoint = hit.point;
		}
		else
		{
			targetPoint = ray.origin + ray.direction * 50f;
		}

		Vector3 dir = targetPoint - transform.position;
		dir.y = 0f;

		if (dir.sqrMagnitude < 0.001f) return;

		Quaternion targetRot = Quaternion.LookRotation(dir);

		characterCore.rotationAux = Quaternion.Slerp(
			characterCore.rotationAux,
			targetRot,
			rotateSpeed * Time.deltaTime
		);
	}
}