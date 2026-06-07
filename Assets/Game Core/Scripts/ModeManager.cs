using UnityEngine;

public class ModeManager : MonoBehaviour
{
    [Header("Fixed Positions for Duel")]
    // Сюда мы вобьем точные координаты верхушек башен
    public Vector3 playerTowerTop = new Vector3(-86f, 20f, 25f); // Координаты для тебя
    public Vector3 botTowerTop = new Vector3(-86f, 20f, 45f);    // Координаты для бота

    [Header("References")]
    public Transform mainSpawnPoint;     // Точка возврата на главный спавн
    public GameObject exitButton;       // Кнопка UI "Выход"
    public GameObject botPrefab;         // Префаб бота

    [Header("State")]
    public bool isInBotMode = false;     // Находимся ли мы в режиме 1 на 1

    private GameObject player;
    private GameObject activeBot;
    private BlocksSpawner spawner;       // Твой основной спавнер

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        spawner = FindFirstObjectByType<BlocksSpawner>();

        if (exitButton != null) exitButton.SetActive(false);
    }

    void Update()
    {
        if (isInBotMode)
        {
            CheckRoundStatus();
        }
    }

    // === ВХОД В РЕЖИМ (вызывается плитой на полу) ===
    public void EnterBotMode()
    {
        if (isInBotMode) return;
        isInBotMode = true;

        // Пересобираем стандартные башни на карте, чтобы они были целыми
        if (spawner != null)
        {
            spawner.SpawnTowers();
        }

        if (exitButton != null) exitButton.SetActive(true);

        RestartRound();
    }

    // === ЗАПУСК / ПЕРЕЗАПУСК РАУНДА ===
    public void RestartRound()
    {
        if (player == null) player = GameObject.FindGameObjectWithTag("Player");

        // Жестко кидаем тебя на вершину первой башни
        if (player != null)
        {
            player.transform.position = playerTowerTop;
        }

        // Чистим старого бота
        if (activeBot != null) Destroy(activeBot);

        // Жестко спавним бота на вершине противоположной башни
        if (botPrefab != null)
        {
            activeBot = Instantiate(botPrefab, botTowerTop, Quaternion.identity);
        }

        Debug.Log("[Дуэль] Раунд начался! Ты на одной башне, бот на другой.");
    }

    private void CheckRoundStatus()
    {
        bool isBotDead = (activeBot == null);
        
        // Если ты упал с башни вниз (высота Y стала слишком низкой, например меньше 0)
        bool isPlayerDead = (player != null && player.transform.position.y < 2f);

        if (isBotDead || isPlayerDead)
        {
            RestartRound();
        }
    }

    // === КНОПКА ВЫХОД ===
    public void TeleportToMainSpawn()
    {
        isInBotMode = false;

        // Удаляем дуэльного бота
        if (activeBot != null) Destroy(activeBot);

        // Возвращаем тебя на спавн острова
        if (player != null && mainSpawnPoint != null)
        {
            player.transform.position = mainSpawnPoint.position;
        }

        if (exitButton != null) exitButton.SetActive(false);

        // Пересобираем башни еще раз для обычной игры
        if (spawner != null) spawner.SpawnTowers();
    }
}