using UnityEngine;

public class DuelTrigger : MonoBehaviour
{
	private void OnTriggerEnter(Collider other)
	{
		if (other.CompareTag("Player") || other.GetComponent<PlayerShooter>() != null)
		{
			DuelManager duelManager = FindFirstObjectByType<DuelManager>();
			if (duelManager != null)
			{
				Debug.Log("[DuelTrigger] Игрок зашел на дуэльную плиту!");
				duelManager.StartDuel();
			}
		}
	}
}