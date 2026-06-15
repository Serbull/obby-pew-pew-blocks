using UnityEngine;

public class DuelTrigger : MonoBehaviour
{
	[Header("Ковер на полу (дочерний текст)")]
	public GameObject myTextObject;

	[Header("Парящий Биллборд (в воздухе)")]
	public GameObject billboardTextObject; // Сюда перетащим наш новый текст

	private void Start()
	{
		if (myTextObject == null)
		{
			var tm = GetComponentInChildren<TMPro.TextMeshPro>();
			if (tm != null) myTextObject = tm.gameObject;
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.CompareTag("Player") || other.GetComponent<PlayerShooter>() != null)
		{
			DuelManager duelManager = FindFirstObjectByType<DuelManager>();
			if (duelManager != null)
			{
				if (duelManager.IsPlayerInDuel()) return;

				Debug.Log("[DuelTrigger] Игрок зашел на дуэльную плиту!");

				// Выключаем ОБА текста
				SetTextActive(false);

				duelManager.StartDuel();
			}
		}
	}

	// Метод включает/выключает и ковер, и биллборд
	public void SetTextActive(bool isActive)
	{
		if (myTextObject != null) myTextObject.SetActive(isActive);
		if (billboardTextObject != null) billboardTextObject.SetActive(isActive);
	}
}