using System.Collections.Generic;
using UnityEngine;

public class DuelManager : MonoBehaviour
{
	[Header("Ссылки на компоненты")]
	public BlocksSpawner spawner;
	public GameObject player;
	public PlayerWeaponEquip weaponEquip;
	public GameObject botPrefab;
	public Transform spawnPoint;

	[Header("Настройки дуэльной зоны")]
	public BoxCollider duelArenaZone;
	public GameObject stopDuelButtonUI;
	public float spawnHeightOffset = 2.0f;

	private GameObject activeDuelBot;
	private bool isDuelActive = false;
	private bool isWaitingForRestart = false;

	private GameController gameController;
	private List<GameObject> myDuelTowers = new List<GameObject>();
	private Coroutine restartCoroutine;

	void Start()
	{
		gameController = FindFirstObjectByType<GameController>();
		if (stopDuelButtonUI != null) stopDuelButtonUI.SetActive(false);
	}

	// Запуск дуэли 1х1
	public void StartDuel()
	{
		if (isDuelActive || isWaitingForRestart) return;
		isDuelActive = true;

		if (gameController != null)
		{
			gameController.ToggleAFK(true);
			var toggle = FindFirstObjectByType<UnityEngine.UI.Toggle>();
			if (toggle != null) toggle.isOn = true;
		}

		if (stopDuelButtonUI != null) stopDuelButtonUI.SetActive(true);

		myDuelTowers = spawner.SpawnTowers(2, duelArenaZone);

		SpawnCharacterOnDuelTower(player, 0);
		if (weaponEquip != null) weaponEquip.EquipWeapon();

		SpawnDuelBot(1);
	}

	// Завершение раунда
	public void EndDuel(bool isPlayerWin, bool clickedStop = false)
	{
		if (!isDuelActive && !clickedStop && !isWaitingForRestart) return;

		// Зачищаем бота
		if (activeDuelBot != null)
		{
			Destroy(activeDuelBot);
			activeDuelBot = null;
		}

		// Зачищаем башни
		if (spawner != null)
		{
			spawner.ClearDuel();
		}
		myDuelTowers.Clear();

		if (clickedStop)
		{
			// Игрок нажал СТОП: полностью выходим из режима дуэли
			isDuelActive = false;
			isWaitingForRestart = false;

			if (restartCoroutine != null)
			{
				StopCoroutine(restartCoroutine);
				restartCoroutine = null;
			}

			if (stopDuelButtonUI != null) stopDuelButtonUI.SetActive(false);

			TeleportPlayerToSpawn();
		}
		else
		{
			// Естественный финиш раунда: просто ждем 3 секунды до следующего боя без всякого UI
			isDuelActive = false;
			isWaitingForRestart = true;

			restartCoroutine = StartCoroutine(WaitAndRestartDuelPhase());
		}
	}

	private System.Collections.IEnumerator WaitAndRestartDuelPhase()
	{
		yield return new WaitForSeconds(3.0f);

		isWaitingForRestart = false;

		// Если за время задержки игрок не вышел кнопкой "Стоп", запускаем заново
		if (stopDuelButtonUI != null && stopDuelButtonUI.activeSelf)
		{
			StartDuel();
		}
	}

	public void OnStopButtonClick()
	{
		EndDuel(false, true);
	}

	public bool CheckAndHandleBotDeath(GameObject bot)
	{
		if (isDuelActive && bot == activeDuelBot)
		{
			EndDuel(true, false);
			return true;
		}
		return false;
	}

	public bool IsPlayerInDuel()
	{
		return isDuelActive || isWaitingForRestart;
	}

	private void SpawnDuelBot(int towerIndex)
	{
		activeDuelBot = Instantiate(botPrefab);
		activeDuelBot.name = "Duel_Bot_Tower_" + towerIndex;

		SpawnCharacterOnDuelTower(activeDuelBot, towerIndex);

		BotShooter botBrain = activeDuelBot.GetComponent<BotShooter>();
		if (botBrain != null)
		{
			botBrain.isDuelBot = true;
			botBrain.InitializeBot(towerIndex);
		}
	}

	private void TeleportPlayerToSpawn()
	{
		if (spawnPoint == null) return;
		CharacterController cc = player.GetComponent<CharacterController>();
		if (cc != null) cc.enabled = false;
		player.transform.position = spawnPoint.position;
		if (cc != null) cc.enabled = true;
		if (weaponEquip != null) weaponEquip.AttachToBack();
	}

	private void SpawnCharacterOnDuelTower(GameObject character, int towerIndex)
	{
		Vector3 teleportPosition = Vector3.zero;
		bool foundBlock = false;

		if (towerIndex < myDuelTowers.Count && myDuelTowers[towerIndex] != null)
		{
			GameObject targetTower = myDuelTowers[towerIndex];

			Transform[] children = targetTower.GetComponentsInChildren<Transform>();
			foreach (var child in children)
			{
				if (child.name == "Duel_FinalBlock_" + towerIndex)
				{
					teleportPosition = child.position + Vector3.up * spawnHeightOffset;
					foundBlock = true;
					break;
				}
			}

			if (!foundBlock)
			{
				float approximateHeight = spawner.towerHeight * spawner.blockSize.y;
				teleportPosition = targetTower.transform.position + Vector3.up * (approximateHeight + spawnHeightOffset);
			}
		}

		if (teleportPosition != Vector3.zero)
		{
			if (character == player)
			{
				CharacterController cc = player.GetComponent<CharacterController>();
				if (cc != null) cc.enabled = false;
				Rigidbody playerRb = player.GetComponent<Rigidbody>();
				if (playerRb != null) { playerRb.linearVelocity = Vector3.zero; playerRb.angularVelocity = Vector3.zero; }
				MonoBehaviour characterCore = player.GetComponent("CharacterCore") as MonoBehaviour;
				if (characterCore != null) characterCore.enabled = false;

				player.transform.position = teleportPosition;

				if (cc != null) cc.enabled = true;
				if (characterCore != null) characterCore.enabled = true;
			}
			else
			{
				character.transform.position = teleportPosition;
			}
		}
	}
}