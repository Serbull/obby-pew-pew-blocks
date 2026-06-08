using System.Collections.Generic;
using UnityEngine;
using TMPro;

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

    // Ссылка на менеджер дуэлей для изоляции режимов
    private DuelManager duelManager;

    void Start()
    {
        // Находим DuelManager на сцене
        duelManager = FindFirstObjectByType<DuelManager>();

        UnityEngine.UI.Toggle afkToggle = FindFirstObjectByType<UnityEngine.UI.Toggle>();
        if (afkToggle != null)
        {
            isAFK = afkToggle.isOn;
        }
        else
        {
            isAFK = true;
        }

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

    // --- ФАЗА 1: Раунд для ботов в лобби/АФК ---
    private void StartLobbyWithBots()
    {
        currentState = GameState.LobbyBotsShooting;
        stateTimer = isFirstRound ? 10f : 60f;
        isPlayerDead = false;

        if (victoryPanelUI != null) victoryPanelUI.SetActive(false);
        if (defeatPanelUI != null) defeatPanelUI.SetActive(false);
        if (centerNotifyText != null) centerNotifyText.text = "";

        // Спавним башни
        spawner.SpawnTowers();

        // Спавним ботов на ВСЕ 4 башни и обязательно ИНИЦИАЛИЗИРУЕМ их через SpawnBotOnTower
        for (int i = 0; i < 4; i++)
        {
            SpawnBotOnTower(i);
        }
    }

    // --- ФАЗА 2: Подготовка перед раундом (5 секунд) ---
    private void StartCountdownPhase()
    {
        currentState = GameState.Countdown;
        stateTimer = 5f;

        isFirstRound = false;

        ClearBotsAndTowers();
        spawner.SpawnTowers();
    }

    // --- ФАЗА 3: Активный раунд с участием Игрока ---
    public void StartActiveGame()
    {
        currentState = GameState.ActiveGame;
        stateTimer = 60f;
        isPlayerDead = false;

        if (centerNotifyText != null) centerNotifyText.text = "";

        List<int> availableTowers = new List<int> { 0, 1, 2, 3 };

        // Выбираем башню для игрока
        int playerTowerIndex = availableTowers[Random.Range(0, availableTowers.Count)];
        availableTowers.Remove(playerTowerIndex);

        // ИЗМЕНЕНИЕ: Если игрок сейчас на дуэли, мы НЕ телепортируем его в главное лобби!
        // И мы НЕ спавним бота на эту вышку, она просто остаётся пустой в этом раунде лобби.
        if (duelManager != null && duelManager.IsPlayerInDuel())
        {
            Debug.Log("[GameController] Игрок на дуэли. Пропускаем его спавн в лобби.");
        }
        else
        {
            SpawnCharacterOnTower(player, playerTowerIndex);
            if (weaponEquip != null) weaponEquip.EquipWeapon();
        }

        // Спавним ботов на оставшиеся 3 вышки главного лобби
        foreach (int botTowerIndex in availableTowers)
        {
            SpawnBotOnTower(botTowerIndex);
        }
    }

    private void OnTimerEnd()
    {
        switch (currentState)
        {
            case GameState.LobbyBotsShooting:
                StartCountdownPhase();
                break;

            case GameState.Countdown:
                if (isAFK)
                {
                    Debug.Log("[GameController] Игрок в АФК. Запускаем раунд ботов на 60 секунд.");
                    StartLobbyWithBots();
                }
                else
                {
                    StartActiveGame();
                }
                break;

            case GameState.ActiveGame:
            case GameState.PlayerDeadButBotsFight:
                EvaluateGameResult();
                break;
        }
    }

    // --- СМЕРТЬ ИГРОКА ---
    public void PlayerDeath()
    {
        // Проверяем: если игрок упал, находясь в дуэли 1х1 — отдаем завершение дуэль-менеджеру
        if (duelManager != null && duelManager.IsPlayerInDuel())
        {
            duelManager.EndDuel(false, false); // Игрок упал -> проиграл дуэль
            return; // Основную игру и ее таймеры не трогаем!
        }

        if (currentState != GameState.ActiveGame) return;

        isPlayerDead = true;
        currentState = GameState.PlayerDeadButBotsFight;

        Debug.Log("[GameController] Игрок упал! Боты достреливаются...");

        if (defeatPanelUI != null) defeatPanelUI.SetActive(true);
        AddCoinsToShop(20);

        TeleportPlayerToSpawn();

        // Проверяем: вдруг пока игрок летел в воду, на поле уже остался всего 1 бот или меньше
        if (activeBots.Count <= 1)
        {
            EvaluateGameResult();
        }
    }

    // --- СМЕРТЬ БОТА ---
    public void BotDeath(GameObject bot)
    {
        // Проверяем: если этот упавший бот принадлежит дуэли — DuelManager сам разберется с матчем
        if (duelManager != null && duelManager.CheckAndHandleBotDeath(bot))
        {
            return; // Завершаем метод, основную игру не трогаем!
        }

        if (!activeBots.Contains(bot)) return;

        activeBots.Remove(bot);
        Destroy(bot);
        Debug.Log($"[GameController] Бот {bot.name} уничтожен! Осталось ботов: {activeBots.Count}");

        // --- ЛОГИКА ЗАВЕРШЕНИЯ: ЕСЛИ ОСТАЛСЯ ВСЕГО 1 БОТ (ИЛИ МЕНЬШЕ) ---
        if (currentState == GameState.LobbyBotsShooting)
        {
            // В режиме АФК дерутся только боты. Если остался 1 — он победил, раунд завершен
            if (activeBots.Count <= 1)
            {
                Debug.Log("[GameController] В раунде ботов остался последний выживший! Конец матча.");
                StartCountdownPhase();
            }
            return;
        }

        if (currentState == GameState.ActiveGame)
        {
            // Если игрок ЕЩЕ ЖИВ, и живых БОТОВ на карте не осталось (0 ботов) — игрок победил
            if (activeBots.Count == 0 && !isPlayerDead)
            {
                WinGame();
            }
        }
        else if (currentState == GameState.PlayerDeadButBotsFight)
        {
            // Если игрок УЖЕ МЕРТВ, и среди ботов выявился один последний выживший (1 бот или 0)
            if (activeBots.Count <= 1)
            {
                EvaluateGameResult();
            }
        }
    }

    private void WinGame()
    {
        currentState = GameState.GameOver;
        stateTimer = 0;
        if (victoryPanelUI != null) victoryPanelUI.SetActive(true);
        AddCoinsToShop(100);
        StartCoroutine(WaitAndRespawn(3.0f));
    }

    private void EvaluateGameResult()
    {
        currentState = GameState.GameOver;
        stateTimer = 0;
        if (!isPlayerDead && activeBots.Count > 0)
        {
            if (defeatPanelUI != null) defeatPanelUI.SetActive(true);
            AddCoinsToShop(20);
        }
        StartCoroutine(WaitAndRespawn(3.0f));
    }

    private System.Collections.IEnumerator WaitAndRespawn(float delay)
    {
        yield return new WaitForSeconds(delay);

        // Выключаем надписи конца игры
        if (victoryPanelUI != null) victoryPanelUI.SetActive(false);
        if (defeatPanelUI != null) defeatPanelUI.SetActive(false);

        // Полностью очищаем старых ботов и башни
        ClearBotsAndTowers();
        TeleportPlayerToSpawn();

        // ВАЖНО: Вместо StopGame() мы НАПРЯМУЮ уходим на нормальную 5-секундную подготовку!
        StartCountdownPhase();
    }

    [ContextMenu("Stop Game")]
    public void StopGame()
    {
        ClearBotsAndTowers();
        TeleportPlayerToSpawn();

        if (victoryPanelUI != null) victoryPanelUI.SetActive(false);
        if (defeatPanelUI != null) defeatPanelUI.SetActive(false);

        // Этот метод теперь используется только при принудительном сбросе или самом старте,
        // возвращая игру в авто-бой ботов
        StartLobbyWithBots();
    }

    public void ToggleAFK(bool value)
    {
        isAFK = value;
        Debug.Log($"[GameController] Режим АФК изменен на: {isAFK}");
    }

    private void SpawnBotOnTower(int towerIndex)
    {
        GameObject botInstance = Instantiate(botPrefab);
        botInstance.name = "Bot_Tower_" + towerIndex;
        activeBots.Add(botInstance);

        SpawnCharacterOnTower(botInstance, towerIndex);

        BotShooter botBrain = botInstance.GetComponent<BotShooter>();
        if (botBrain != null)
        {
            // Передаем правильный индекс башни, чтобы бот знал, кто он, и видел врагов!
            botBrain.InitializeBot(towerIndex);
        }
    }

    private void ClearBotsAndTowers()
    {
        foreach (GameObject bot in activeBots)
        {
            if (bot != null) Destroy(bot);
        }
        activeBots.Clear();
        if (spawner != null) spawner.Clear();
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

    private void UpdateUI()
    {
        int roundedTime = Mathf.CeilToInt(stateTimer);

        switch (currentState)
        {
            case GameState.LobbyBotsShooting:
                if (topTimerText != null) topTimerText.text = $"До конца раунда: {roundedTime} сек";
                if (centerNotifyText != null) centerNotifyText.text = isAFK ? "Вы в режиме AFK" : "";
                break;

            case GameState.Countdown:
                if (topTimerText != null) topTimerText.text = "Подготовка...";
                if (centerNotifyText != null) centerNotifyText.text = $"Игра начнётся через {roundedTime} секунд";
                break;

            case GameState.ActiveGame:
            case GameState.PlayerDeadButBotsFight:
                if (topTimerText != null) topTimerText.text = $"Время раунда: {roundedTime} сек";
                if (centerNotifyText != null) centerNotifyText.text = isPlayerDead ? "Вы выбыли! Наблюдение..." : "";
                break;
        }
    }

    private void AddCoinsToShop(int amount)
    {
        int totalCoins = PlayerPrefs.GetInt("Coins", 0) + amount;
        PlayerPrefs.SetInt("Coins", totalCoins);
        PlayerPrefs.Save();

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

        // ИЗМЕНЕНИЕ: Ищем башню СТРОГО среди главных башен основного матча, 
        // чтобы случайно не залезть на дуэльные вышки 1х1
        string targetTowerName = "Tower_" + towerIndex;
        GameObject targetTower = GameObject.Find(targetTowerName);

        // Проверяем, что эта башня не принадлежит дуэли (у дуэльных башен корень лежит в spawner.ClearDuel)
        // Для надежности просто пробежимся по дочерним объектам найденной главной башни
        if (targetTower != null)
        {
            Transform[] children = targetTower.GetComponentsInChildren<Transform>();
            foreach (var child in children)
            {
                if (child.name == "FinalBlock_" + towerIndex)
                {
                    teleportPosition = child.position + Vector3.up * spawnHeightOffset;
                    foundBlock = true;
                    break;
                }
            }
        }

        // Если блок по какой-то причине не найден в иерархии, считаем математически над ГЛАВНОЙ башней
        if (!foundBlock && targetTower != null)
        {
            float approximateHeight = spawner.towerHeight * spawner.blockSize.y;
            teleportPosition = targetTower.transform.position + Vector3.up * (approximateHeight + spawnHeightOffset);
        }
        else if (targetTower == null)
        {
            // Совсем крайний случай, если башни еще не успели создаться
            teleportPosition = spawnPoint.position;
        }

        // Сам телепорт персонажа (твой оригинальный код без изменений)
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