using UnityEngine;

public class PlayerAimRotation : MonoBehaviour
{
	public Camera playerCamera;
	public Animator animator;
	public CharacterCore characterCore;

	public float rotateSpeed = 15f;

	[Header("Настройки фильтрации")]
	[Tooltip("Выберите слой, на котором находится ваш Player (например, 'Player')")]
	public LayerMask excludeLayers; // Переменная для маски слоёв

  private  void Start()
    {
        
    }

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

		// 1. Создаем маску для слоев, которые нужно НАПРАВЛЕННО ИГНОРИРОВАТЬ
		// Игнорируем слой Character (на котором висит игрок) и слой UI (на котором висит текстовый Canvas)
		int ignoreMask = LayerMask.GetMask("Character", "UI");

		// 2. Инвертируем маску (~), чтобы Physics.Raycast сталкивался со ВСЕМ, КРОМЕ этих двух слоев
		if (Physics.Raycast(ray, out hit, 500f, ~ignoreMask))
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