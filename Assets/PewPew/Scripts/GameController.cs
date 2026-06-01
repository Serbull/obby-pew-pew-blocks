using UnityEngine;

public class GameController : MonoBehaviour
{
	[Header("Ссылки на объекты")]
	[Tooltip("Перетащи сюда объект, где висит BlocksSpawner")]
	public BlocksSpawner spawner;

	[Tooltip("Перетащи сюда твоего Игрока (персонажа) со сцены")]
	public GameObject player;

	[Tooltip("Перетащи сюда саму кнопку Play, чтобы она исчезала после старта")]
	public GameObject playButtonUI;

	[Header("Настройки телепорта")]
	[Tooltip("Высота над башней, чтобы игрок не застрял ногами в блоках")]
	public float spawnHeightOffset = 2.0f;

	public void StartGame()
	{
		// Проверяем, всё ли ты перетащил в инспекторе
		if (spawner == null || player == null)
		{
			Debug.LogError("GameController: Перетащи Скрипт-Спавнер и Игрока в слоты инспектора!");
			return;
		}

		// 1. Спавним башни с новыми ровными блоками и сдвигами
		spawner.SpawnTowers();

		// 2. Выбираем случайную башню от 0 до 3
		int randomTowerIndex = Random.Range(0, 4);

		// Ищем один из маркированных верхних блоков на выбранной башне
		string targetBlockName = "FinalBlock_" + randomTowerIndex;
		GameObject targetBlock = GameObject.Find(targetBlockName);

		if (targetBlock != null)
		{
			// Считаем точную точку: позиция физического блока + смещение вверх
			Vector3 teleportPosition = targetBlock.transform.position + Vector3.up * spawnHeightOffset;
			TeleportPlayer(teleportPosition);
		}
		else
		{
			// Запасной математический вариант, если имя блока почему-то не нашлось
			GameObject targetTower = GameObject.Find("Tower_" + randomTowerIndex);
			if (targetTower != null)
			{
				float approximateHeight = spawner.towerHeight * spawner.blockSize.y;
				Vector3 teleportPosition = targetTower.transform.position + Vector3.up * (approximateHeight + spawnHeightOffset);
				TeleportPlayer(teleportPosition);
			}
		}

		// 3. Прячем кнопку Play, чтобы можно было спокойно играть
		if (playButtonUI != null)
		{
			playButtonUI.SetActive(false);
		}
	}

	private void TeleportPlayer(Vector3 targetPos)
	{
		// Выключаем CharacterController, если он используется
		CharacterController cc = player.GetComponent<CharacterController>();
		if (cc != null) cc.enabled = false;

		// На всякий случай гасим скорость Rigidbody перед телепортом, чтобы старая инерция не швыряла игрока
		Rigidbody playerRb = player.GetComponent<Rigidbody>();
		if (playerRb != null)
		{
			playerRb.linearVelocity = Vector3.zero;
			playerRb.angularVelocity = Vector3.zero;
		}

		// Выключаем скрипт ходьбы на долю секунды, чтобы его внутренний GroundSnap не сработал раньше времени
		MonoBehaviour characterCore = player.GetComponent("CharacterCore") as MonoBehaviour;
		if (characterCore != null) characterCore.enabled = false;

		// Сам телепорт
		player.transform.position = targetPos;

		// Включаем компоненты обратно
		if (cc != null) cc.enabled = true;
		if (characterCore != null) characterCore.enabled = true;

		Debug.Log($"[GameController] Игрок успешно телепортирован на ровный блок башни №{targetPos}!");
	}
}