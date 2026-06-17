using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Serbull.GameAssets;
using YG;

public class GameController : MonoBehaviour
{
    public enum GameState { LobbyBotsShooting, Countdown, ActiveGame, PlayerDeadButBotsFight, GameOver }
    private GameState currentState;

    [Header("Ссылки на объекты")]
    public BlocksSpawner spawner;
    public GameObject player;
    public PlayerWeaponEquip weaponEquip;
    public GameObject botPrefab;
    public Transform spawnPoint;

    [Header("Настройки UI Текстов")]
    public TextMeshProUGUI topTimerText;
    public TextMeshProUGUI centerNotifyText;
    public TextMeshProUGUI afkNotifyText; // НОВАЯ ПЕРЕМЕННАЯ: Сюда перетащи новый отдельный текст из Canvas
    public GameObject afkButton; // Кнопка АФК — дизейблится, пока игрок в активной игре

    [Header("Настройки телепорта")]
    public float spawnHeightOffset = 2.0f;

    [Header("Экран Конца Игры (UI)")]
    public GameObject victoryPanelUI;
    public GameObject defeatPanelUI;

    [Header("Ссылка на Экономику")]
    public ShopManager shopManager;

    private List<GameObject> activeBots = new List<GameObject>();
    private float stateTimer;
    private bool isPlayerDead = false;
    private bool isAFK = false;
    private bool isFirstRound = true;

    private int currentMatchPlayersCount = 4;
    private DuelManager duelManager;

    void Start()
    {
        duelManager = FindFirstObjectByType<DuelManager>();

        // Состояние AFK задаётся кнопкой AFKToggle (она вызывает ToggleAFK в своём Start).
        // Здесь оставляем дефолт из поля isAFK (false), чтобы игрок по умолчанию заходил в игру.

        AddCoinsToShop(0);
        StartLobbyWithBots();
    }

    void Update()
    {
        if (stateTimer > 0)
        {
            stateTimer -= Time.deltaTime;
            UpdateUI();

            if (stateTimer <= 0)
            {
                OnTimerEnd();
            }
        }
    }

    // Переключалка AFK режима (вызывается из Toggle в UI)
    public void ToggleAFK(bool value)
    {
        isAFK = value;
        UpdateAFKTextVisibility(); // Автоматически обновляем визуал при изменении флага
    }

    // Независимый метод управления текстом АФК
    private void UpdateAFKTextVisibility()
    {
        if (afkNotifyText != null)
        {
            if (isAFK)
            {
                afkNotifyText.text = Services.Localization.GetText("ui_afk_mode");
                afkNotifyText.gameObject.SetActive(true);
            }
            else
            {
                afkNotifyText.gameObject.SetActive(false);
            }
        }
    }

    public void ClearCenterText()
    {
        if (centerNotifyText != null) centerNotifyText.text = "";
    }

    // --- ОБНОВЛЕННЫЙ МЕТОД UI: ТЕПЕРЬ БЕЗ КАКИХ-ЛИБО СЛЕДОВ АФК ---
    private void UpdateUI()
    {
        int roundedTime = Mathf.CeilToInt(stateTimer);

        switch (currentState)
        {
            case GameState.LobbyBotsShooting:
                if (topTimerText != null)
                {
                    string rawText = Services.Localization.GetText("ui_round_end");
                    topTimerText.text = string.Format(rawText, roundedTime);
                }
                // Логика центрального текста здесь теперь пустая для лобби,
                // так как АФК живет своей жизнью в отдельном объекте
                break;

            case GameState.Countdown:
                if (topTimerText != null)
                {
                    topTimerText.text = Services.Localization.GetText("ui_countdown_title");
                }
                if (centerNotifyText != null)
                {
                    if (duelManager != null && duelManager.IsPlayerInDuel()) return;

                    string rawText = Services.Localization.GetText("ui_game_start_delay");
                    centerNotifyText.text = string.Format(rawText, roundedTime);
                }
                break;

            case GameState.ActiveGame:
            case GameState.PlayerDeadButBotsFight:
                if (topTimerText != null)
                {
                    string rawText = Services.Localization.GetText("ui_round_time");
                    topTimerText.text = string.Format(rawText, roundedTime);
                }
                if (centerNotifyText != null)
                {
                    if (duelManager != null && duelManager.IsPlayerInDuel()) return;

                    centerNotifyText.text = isPlayerDead ? Services.Localization.GetText("ui_player_eliminated") : "";
                }
                break;
        }
    }

    // ОСТАЛЬНОЙ ТВОЙ КОД ОСТАЕТСЯ БЕЗ ИЗМЕНЕНИЙ...
    private void StartLobbyWithBots()
    {
        currentState = GameState.LobbyBotsShooting;
        stateTimer = isFirstRound ? 10f : 90f;
        isPlayerDead = false;

        if (victoryPanelUI != null) victoryPanelUI.SetActive(false);
        if (defeatPanelUI != null) defeatPanelUI.SetActive(false);
        if (centerNotifyText != null) centerNotifyText.text = "";

        if (isFirstRound)
        {
            currentMatchPlayersCount = Mathf.Clamp(4 + Mathf.FloorToInt(Mathf.Pow(Random.value, 1.5f) * 5f), 4, 8);
            spawner.SpawnTowers(currentMatchPlayersCount);
        }

        for (int i = 0; i < currentMatchPlayersCount; i++) { SpawnBotOnTower(i); }
    }

    private void StartCountdownPhase()
    {
        currentState = GameState.Countdown;
        stateTimer = 5f;
        isFirstRound = false;
        if (victoryPanelUI != null) victoryPanelUI.SetActive(false);
        if (defeatPanelUI != null) defeatPanelUI.SetActive(false);
        ClearBotsAndTowers();
        currentMatchPlayersCount = Mathf.Clamp(4 + Mathf.FloorToInt(Mathf.Pow(Random.value, 1.5f) * 5f), 4, 8);
        spawner.SpawnTowers(currentMatchPlayersCount);
    }

    public void StartActiveGame()
    {
        currentState = GameState.ActiveGame;
        stateTimer = 90f;
        isPlayerDead = false;
        if (centerNotifyText != null) centerNotifyText.text = "";

        List<int> availableTowers = new List<int>();
        for (int i = 0; i < currentMatchPlayersCount; i++) { availableTowers.Add(i); }

        int playerTowerIndex = availableTowers[Random.Range(0, availableTowers.Count)];
        availableTowers.Remove(playerTowerIndex);

        // Если начался новый раунд, а игрок всё ещё в дуэли — завершаем дуэль и заводим игрока в игру
        if (duelManager != null && duelManager.IsPlayerInDuel())
        {
            duelManager.EndDuel(false, true);
        }

        // Игрок зашёл в игру — дизейблим кнопку АФК
        if (afkButton != null) afkButton.SetActive(false);

        SpawnCharacterOnTower(player, playerTowerIndex);
        if (weaponEquip != null) weaponEquip.EquipWeapon();

        foreach (int botTowerIndex in availableTowers) { SpawnBotOnTower(botTowerIndex); }
    }

    private void OnTimerEnd()
    {
        switch (currentState)
        {
            case GameState.LobbyBotsShooting: StartCountdownPhase(); break;
            case GameState.Countdown: if (isAFK) StartLobbyWithBots(); else StartActiveGame(); break;
            case GameState.ActiveGame:
            case GameState.PlayerDeadButBotsFight: EvaluateGameResult(); break;
        }
    }

    public void PlayerDeath()
    {
        if (duelManager != null && duelManager.IsPlayerInDuel()) { duelManager.EndDuel(false, false); return; }
        if (currentState != GameState.ActiveGame) return;

        isPlayerDead = true;
        currentState = GameState.PlayerDeadButBotsFight;

        if (defeatPanelUI != null)
        {
            defeatPanelUI.SetActive(true);
            StartCoroutine(HidePanelAfterDelay(defeatPanelUI, 3.0f));
        }

        AddCoinsToShop(20);
        TeleportPlayerToSpawn();

        if (activeBots.Count <= 1) EvaluateGameResult();
    }

    public void BotDeath(GameObject bot)
    {
        if (duelManager != null && duelManager.CheckAndHandleBotDeath(bot)) return;
        if (!activeBots.Contains(bot)) return;

        string botName = bot.name;
        if (botName.StartsWith("Bot_Tower_"))
        {
            string indexStr = botName.Replace("Bot_Tower_", "");
            if (int.TryParse(indexStr, out int towerIdx)) { if (spawner != null) spawner.ClearSingleTower(towerIdx); }
        }

        activeBots.Remove(bot);
        Destroy(bot);

        if (currentState == GameState.LobbyBotsShooting)
        {
            if (activeBots.Count <= 1) StartCountdownPhase();
            return;
        }

        if (currentState == GameState.ActiveGame)
        {
            if (activeBots.Count == 0 && !isPlayerDead) WinGame();
        }
        else if (currentState == GameState.PlayerDeadButBotsFight)
        {
            if (activeBots.Count <= 1) EvaluateGameResult();
        }
    }

    private void WinGame()
    {
        currentState = GameState.GameOver;
        stateTimer = 0;
        if (victoryPanelUI != null)
        {
            victoryPanelUI.SetActive(true);
            StartCoroutine(HidePanelAfterDelay(victoryPanelUI, 3.0f));
        }
        AddCoinsToShop(100);
        StartCoroutine(WaitAndRespawn(3.0f));
        SaveManager.Data.wins++;
        Leaderboards.Send("wins", SaveManager.Data.wins);
    }

    private void EvaluateGameResult()
    {
        currentState = GameState.GameOver;
        stateTimer = 0;
        if (!isPlayerDead && activeBots.Count > 0)
        {
            if (defeatPanelUI != null)
                defeatPanelUI.SetActive(true);
            AddCoinsToShop(20);
        }
        StartCoroutine(WaitAndRespawn(3.0f));
    }

    private System.Collections.IEnumerator HidePanelAfterDelay(GameObject panel, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (panel != null) panel.SetActive(false);
    }

    private System.Collections.IEnumerator WaitAndRespawn(float delay)
    {
        yield return new WaitForSeconds(delay);
        ClearBotsAndTowers();
        TeleportPlayerToSpawn();
        StartCountdownPhase();
    }

    // [ContextMenu("Stop Game")]
    // public void StopGame()
    // {
    //     isFirstRound = true;
    //     ClearBotsAndTowers();
    //     TeleportPlayerToSpawn();
    //     if (victoryPanelUI != null) victoryPanelUI.SetActive(false);
    //     if (defeatPanelUI != null) defeatPanelUI.SetActive(false);
    //     StartLobbyWithBots();
    // }

    private void SpawnBotOnTower(int towerIndex)
    {
        GameObject botInstance = Instantiate(botPrefab);
        botInstance.name = "Bot_Tower_" + towerIndex;
        activeBots.Add(botInstance);
        SpawnCharacterOnTower(botInstance, towerIndex);
        if (botInstance.TryGetComponent<BotShooter>(out var botBrain))
            botBrain.InitializeBot(towerIndex);
    }

    private void ClearBotsAndTowers()
    {
        foreach (GameObject bot in activeBots) { if (bot != null) Destroy(bot); }
        activeBots.Clear();
        if (currentState == GameState.Countdown || currentState == GameState.GameOver) { if (spawner != null) spawner.Clear(); }
        ClearAllBullets();
    }

    private void ClearAllBullets()
    {
        GameObject[] allObjects = FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        int deletedCount = 0;
        foreach (GameObject go in allObjects)
        {
            if (go != null && (go.name.Contains("Bullet") || go.CompareTag("Bullet"))) { Destroy(go); deletedCount++; }
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

        if (afkButton != null) afkButton.SetActive(true);
        YG2.InterstitialAdvShow();
    }

    private void AddCoinsToShop(int amount)
    {
        SaveManager.Data.coins += amount;
        SaveManager.SaveGameData();
        long totalCoins = SaveManager.Data.coins;
        if (shopManager != null) shopManager.Start();

        TMPro.TextMeshProUGUI[] allTexts = FindObjectsByType<TMPro.TextMeshProUGUI>(FindObjectsSortMode.None);
        foreach (var txt in allTexts)
        {
            if ((txt.gameObject.name.Contains("Coin") || txt.transform.parent.name.Contains("Coin")) && !txt.gameObject.name.Contains("Play"))
            {
                txt.text = totalCoins.ToString();
            }
        }
    }

    private void SpawnCharacterOnTower(GameObject character, int towerIndex)
    {
        Vector3 teleportPosition = Vector3.zero;
        bool foundBlock = false;
        string targetTowerName = "Tower_" + towerIndex;
        GameObject targetTower = GameObject.Find(targetTowerName);

        if (targetTower != null)
        {
            Transform[] children = targetTower.GetComponentsInChildren<Transform>();
            foreach (var child in children)
            {
                if (child.name == "FinalBlock_" + towerIndex) { teleportPosition = child.position + Vector3.up * spawnHeightOffset; foundBlock = true; break; }
            }
        }

        if (!foundBlock && targetTower != null)
        {
            float approximateHeight = spawner.towerHeight * spawner.blockSize.y;
            teleportPosition = targetTower.transform.position + Vector3.up * (approximateHeight + spawnHeightOffset);
        }
        else if (targetTower == null) { teleportPosition = spawnPoint.position; }

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
        else { character.transform.position = teleportPosition; }
    }
}