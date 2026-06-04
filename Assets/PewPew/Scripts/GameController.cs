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

    [Tooltip("Перетащи сюда саму кнопку Play (объект Play из папки HUD)")]
    public GameObject playButtonUI;

    [Tooltip("Spawn Point, что бы телепортироваться на спавн")]
    public Transform spawnPoint;

    [Header("Настройки телепорта")]
    [Tooltip("Высота над башней, чтобы никто не застрял ногами в блоках")]
    public float spawnHeightOffset = 2.0f;

    [Header("Экран Конца Игры (UI)")]
    [Tooltip("Перетащи сюда объект панели ПОБЕДЫ")]
    public GameObject victoryPanelUI;

    [Tooltip("Перетащи сюда объект панели ПРОИГРЫША")]
    public GameObject defeatPanelUI;

    [Header("Ссылка на Экономику")]
    [Tooltip("Перетащи сюда UIManager, на котором висит ShopManager")]
    public ShopManager shopManager;

    // Список для отслеживания заспавненных ботов
    private List<GameObject> activeBots = new List<GameObject>();
    
    // Флаг, чтобы раунд не завершался несколько раз одновременно
    private bool isGameActive = false;

    public void StartGame()
    {
        if (spawner == null || player == null || botPrefab == null || shopManager == null)
        {
            Debug.LogError("GameController: Заполни все слоты в инспекторе!");
            return;
        }

        // --- ДОБАВЛЯЕМ ЭТУ СТРОЧКУ СЮДА ---
        // Обновляем UI актуальным балансом из памяти прямо в момент нажатия на Play
        AddCoinsToShop(0); 
        // ----------------------------------

        // Мгновенно прячем экраны конца игры при старте нового раунда
        if (victoryPanelUI != null) victoryPanelUI.SetActive(false);
        if (defeatPanelUI != null) defeatPanelUI.SetActive(false);

        // Очищаем старых ботов и башни перед стартом
        StopGame();

        isGameActive = true;

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

    // --- ФУНКЦИЯ ОЧИСТКИ УРОВНЯ И ВОЗВРАТА НА СПАВН ---
    [ContextMenu("Stop Game")] 
    public void StopGame()
    {
        // Удаляем всех ботов со сцены
        foreach (GameObject bot in activeBots)
        {
            if (bot != null) Destroy(bot);
        }
        activeBots.Clear();

        // Возвращаем кнопку Play на экран
        if (playButtonUI != null) playButtonUI.SetActive(true);

        // Телепортируем игрока на спавн
        if (spawnPoint != null)
        {
            CharacterController cc = player.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;

            player.transform.position = spawnPoint.position;

            if (cc != null) cc.enabled = true;

            if (weaponEquip != null) weaponEquip.AttachToBack();

            // Очищаем башни Jenga
            BlocksSpawner blocksSpawner = FindFirstObjectByType<BlocksSpawner>();
            if (blocksSpawner != null) blocksSpawner.Clear();
        }

        Debug.Log("[GameController] Раунд полностью сброшен, сцена зачищена.");
    }

    // --- ФУНКЦИЯ СМЕРТИ ОДНОГО БОТА ---
    public void BotDeath(GameObject bot)
    {
        if (bot == null || !isGameActive) return;

        if (activeBots.Contains(bot))
        {
            activeBots.Remove(bot);
        }

        Destroy(bot);
        Debug.Log($"[GameController] Бот {bot.name} уничтожен!");

        // Если живых ботов не осталось — игрок ПОБЕДИЛ!
        if (activeBots.Count == 0)
        {
            WinGame();
        }
    }

    // --- ФУНКЦИЯ ПРОИГРЫША ИГРОКА ---
    public void PlayerDeath()
    {
        if (!isGameActive) return;
        isGameActive = false; 

        Debug.Log("[GameController] ИГРОК УПАЛ В ВОДУ! Оформляем проигранный раунд...");

        // Включаем надпись поражения сразу в воде
        if (defeatPanelUI != null) defeatPanelUI.SetActive(true);

        // Начисляем монеты за проигрыш
        AddCoinsToShop(20);

        // Запускаем таймер на 3 секунды
        StartCoroutine(WaitAndRespawn(3.0f));
    }

    private void WinGame()
    {
        isGameActive = false;
        Debug.Log("[GameController] ПОБЕДА! Все боты повержены.");

        // Включаем надпись победы
        if (victoryPanelUI != null) victoryPanelUI.SetActive(true);

        // Насыпаем куш за победу
        AddCoinsToShop(100);

        // Запускаем таймер на 3 секунды
        StartCoroutine(WaitAndRespawn(3.0f));
    }

    // Корутина ожидания: держит надпись 3 секунды, гасит её и сбрасывает раунд
    private System.Collections.IEnumerator WaitAndRespawn(float delay)
    {
        yield return new WaitForSeconds(delay);

        // Выключаем надписи конца игры перед спавном
        if (victoryPanelUI != null) victoryPanelUI.SetActive(false);
        if (defeatPanelUI != null) defeatPanelUI.SetActive(false);

        // Чистим карту и телепортируем игрока домой
        StopGame(); 
    }

    // Метод начисления монет и обновления ВСЕГО UI на сцене
    private void AddCoinsToShop(int amount)
    {
        // Читаем баланс из сохранений (если там пусто, то дефолт 0, никаких скрытых 1000)
        int totalCoins = PlayerPrefs.GetInt("Coins", 0);
        totalCoins += amount;

        // Перезаписываем в память устройства
        PlayerPrefs.SetInt("Coins", totalCoins);
        PlayerPrefs.Save();

        Debug.Log($"[Экономика] Деньги начислены. Новый баланс в памяти: {totalCoins}");

        // Синхронизируем магазин пушек, если он привязан
        if (shopManager != null)
        {
            shopManager.Start(); 
        }

        // Обновляем вообще все UI счетчики монет на экране
        TMPro.TextMeshProUGUI[] allTexts = FindObjectsByType<TMPro.TextMeshProUGUI>(FindObjectsSortMode.None);
        foreach (var txt in allTexts)
        {
            if (txt.gameObject.name.Contains("Coin") || txt.transform.parent.name.Contains("Coin") || txt.gameObject.name.Contains("Play"))
            {
                if (!txt.gameObject.name.Contains("Play")) 
                {
                    txt.text = totalCoins.ToString();
                }
            }
        }
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