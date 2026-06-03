using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
	[Header("Ссылки на объекты")]
	[Tooltip("Перетащи сюда объект, где висит BlocksSpawner")]
	public BlocksSpawner spawner;

	[Tooltip("Перетащи сюда твоего Игрока (персонажа) со сцены")]
	public GameObject player;

	[Tooltip("Player с WeaponEquip")]
	public PlayerWeaponEquip weaponEquip;

	[Tooltip("Преаб Бота")]
	public GameObject botPrefab;

	[Tooltip("Перетащи сюда саму кнопку Play, чтобы она исчезала после старта")]
	public GameObject playButtonUI;

	[Tooltip("Spawn Point, что бы телепортироваться на спавн")]
	public Transform spawnPoint;

	[Header("Настройки телепорта")]
	[Tooltip("Высота над башней, чтобы никто не застрял ногами в блоках")]
	public float spawnHeightOffset = 2.0f;

	// Список для отслеживания заспавненных ботов
	private List<GameObject> activeBots = new List<GameObject>();

	public void StartGame()
	{
		if (spawner == null || player == null || botPrefab == null)
		{
			Debug.LogError("GameController: Заполни все слоты в инспекторе (включая Bot Prefab)!");
			return;
		}

		// Очищаем старых ботов на всякий случай, если они остались
		StopGame();

		// 1. Спавним 4 башни
		spawner.SpawnTowers();

		List<int> availableTowers = new List<int> { 0, 1, 2, 3 };

		// 2. Выбираем случайную башню ДЛЯ ИГРОКА
		int playerTowerIndex = availableTowers[Random.Range(0, availableTowers.Count)];
		availableTowers.Remove(playerTowerIndex);

		SpawnCharacterOnTower(player, playerTowerIndex);

		if (weaponEquip != null) weaponEquip.EquipWeapon();

		// 3. Спавним БОТОВ на оставшиеся 3 башни
		foreach (int botTowerIndex in availableTowers)
		{
			GameObject botInstance = Instantiate(botPrefab);
			botInstance.name = "Bot_Tower_" + botTowerIndex;

			// Добавляем бота в наш список контроля
			activeBots.Add(botInstance);

			SpawnCharacterOnTower(botInstance, botTowerIndex);

			BotShooter botBrain = botInstance.GetComponent<BotShooter>();
			if (botBrain != null)
			{
				botBrain.InitializeBot(botTowerIndex);
			}
		}

		// Прячем кнопку Play
		if (playButtonUI != null) playButtonUI.SetActive(false);
	}

	// --- ФУНКЦИЯ ЗАВЕРШЕНИЯ ИГРЫ ---
	[ContextMenu("Stop Game")] // Можно потестить через три точки в инспекторе
	public void StopGame()
	{
		// 1. Удаляем всех ботов из списка и очищаем сцену
		foreach (GameObject bot in activeBots)
		{
			if (bot != null)
			{
				// Если у бота в руках было создано оружие, оно удалится вместе с ним автоматически, 
				// так как является его дочерним объектом (благодаря SetParent в PlayerWeaponEquip)
				Destroy(bot);
			}
		}
		activeBots.Clear();

		// 2. Возвращаем кнопку Play на экран, чтобы начать заново
		if (playButtonUI != null)
		{
			playButtonUI.SetActive(true);
		}

		if (spawnPoint != null)
		{
			// На время перемещения отключаем CharacterController игрока, если он есть
			CharacterController cc = player.GetComponent<CharacterController>();
			if (cc != null) cc.enabled = false;

			// Телепортируем на спавн
			player.transform.position = spawnPoint.position;

			// Включаем обратно
			if (cc != null) cc.enabled = true;

			if (weaponEquip != null)
			{
				weaponEquip.AttachToBack();
			}
			else
			{
				Debug.LogError("На объекте Player не найден скрипт PlayerWeaponEquip!");
			}

			BlocksSpawner blocksSpawner = FindFirstObjectByType<BlocksSpawner>();

			if (blocksSpawner != null)
			{
				blocksSpawner.Clear();
			}
			else
			{
				Debug.LogError("На объекте ArenaZone не найден скрипт BlocksSpawner!");
			}

			Debug.Log("Игрок успешно возрожден!");
		}
		else
		{
			Debug.LogError("Ошибка: Забыл перетащить SpawnPoint в инспектор воды!");
		}

		Debug.Log("[GameController] Игра остановлена, боты полностью удалены!");
	}

	// --- ФУНКЦИЯ СМЕРТИ ОДНОГО БОТА ---
	public void BotDeath(GameObject bot)
	{
		if (bot == null) return;

		// Если этот бот есть в нашем списке активных, убираем его оттуда
		if (activeBots.Contains(bot))
		{
			activeBots.Remove(bot);
		}

		// Удаляем бота со сцены
		Destroy(bot);
		Debug.Log($"[GameController] Бот {bot.name} упал в воду и был уничтожен!");

		// ТУТ ЛОГИКА НА БУДУЩЕЕ: 
		// Если хочешь, чтобы игра заканчивалась, когда остался один игрок:
		// if (activeBots.Count == 0) { Написать "Победа!"; }
	}

	private void SpawnCharacterOnTower(GameObject character, int towerIndex)
	{
		string targetBlockName = "FinalBlock_" + towerIndex;
		GameObject targetBlock = GameObject.Find(targetBlockName);

		Vector3 teleportPosition;

		if (targetBlock != null)
		{
			teleportPosition = targetBlock.transform.position + Vector3.up * spawnHeightOffset;
		}
		else
		{
			GameObject targetTower = GameObject.Find("Tower_" + towerIndex);
			float approximateHeight = spawner.towerHeight * spawner.blockSize.y;
			teleportPosition = targetTower.transform.position + Vector3.up * (approximateHeight + spawnHeightOffset);
		}

		if (character == player)
		{
			TeleportPlayer(teleportPosition);
		}
		else
		{
			character.transform.position = teleportPosition;
		}
	}

	private void TeleportPlayer(Vector3 targetPos)
	{
		CharacterController cc = player.GetComponent<CharacterController>();
		if (cc != null) cc.enabled = false;

		Rigidbody playerRb = player.GetComponent<Rigidbody>();
		if (playerRb != null)
		{
			playerRb.linearVelocity = Vector3.zero;
			playerRb.angularVelocity = Vector3.zero;
		}

		MonoBehaviour characterCore = player.GetComponent("CharacterCore") as MonoBehaviour;
		if (characterCore != null) characterCore.enabled = false;

		player.transform.position = targetPos;

		if (cc != null) cc.enabled = true;
		if (characterCore != null) characterCore.enabled = true;
	}
}