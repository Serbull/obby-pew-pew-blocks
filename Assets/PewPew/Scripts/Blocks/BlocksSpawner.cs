using UnityEngine;
using System.Collections.Generic;

public class BlocksSpawner : MonoBehaviour
{
    public GameObject blockPrefab;
    public BoxCollider arenaZone;

    [Header("Jenga Settings")]
    public int towerHeight = 15;
    public int blocksPerRow = 3;
    public Vector3 blockSize = new Vector3(3.6f, 1f, 1.2f);
    public float gap = 0.02f;

    [Header("Floor Colors")]
    public Color[] floorColors = new Color[] { Color.red, Color.green, Color.blue };
    [Range(0f, 1f)] public float darkenAmount = 0.2f; // Насколько затемнять часть блоков (0.2 = на 20%)

    [Header("Random Tilt Settings")]
    [Range(0, 10)] public int minTiltedFloors = 2;
    [Range(0, 10)] public int maxTiltedFloors = 4;
    public float minTiltAngle = 30f;
    public float maxTiltAngle = 45f;
    public bool allowNegativeTilt = true;

    [Header("Настройки Динамического Спавна")]
    public float minDistanceBetweenTowers = 12f; // Минимальное расстояние между башнями, чтобы не слипались
    public float arenaEdgeOffset = 4f;            // Отступ от краев зоны, чтобы башни не застревали в стенах

    private readonly List<GameObject> mainTowers = new();
    private readonly List<GameObject> duelTowers = new();

    // count теперь задается динамически из GameController (от 4 до 8)
    public List<GameObject> SpawnTowers(int count = 4, BoxCollider targetZone = null)
    {
        BoxCollider activeZone = targetZone != null ? targetZone : arenaZone;
        Bounds b = activeZone.bounds;
        List<GameObject> createdTowers = new();


        if (count == 2)
        {
            // Очищаем старые дуэльные башни
            ClearDuel();

            // Вычисляем ИДЕАЛЬНУЮ высоту пола для дуэльной зоны (точно так же, как в основном режиме)
            float spawnY = b.min.y + (blockSize.y / 2f);

            float offset = b.size.x * 0.25f;

            // Собираем базовые точки: берем X и Z от центра с нужным смещением, а Y — строго ПОЛ арены
            Vector3[] bases = {
                new Vector3(b.center.x - offset, spawnY, b.center.z),
                new Vector3(b.center.x + offset, spawnY, b.center.z)
            };

            for (int i = 0; i < 2; i++)
            {
                GameObject tower = SpawnJengaTower(bases[i], i, activeZone, duelTowers, "Duel_Tower_", "Duel_FinalBlock_");
                createdTowers.Add(tower);
            }
        }
        else
        {
            // ОСНОВНОЙ РЕЖИМ (Генерация от 4 до 8 башен в случайных местах)
            Clear();

            List<Vector3> spawnedPositions = new List<Vector3>();

            for (int i = 0; i < count; i++)
            {
                Vector3 randomPos = GetRandomValidPosition(b, spawnedPositions);
                GameObject tower = SpawnJengaTower(randomPos, i, activeZone, mainTowers, "Tower_", "FinalBlock_");
                createdTowers.Add(tower);
                spawnedPositions.Add(randomPos);
            }
        }

        return createdTowers;
    }

    // Вспомогательный метод поиска случайной позиции с проверкой дистанции
    private Vector3 GetRandomValidPosition(Bounds bounds, List<Vector3> existingPositions)
    {
        // Базовая высота спавна (по низу коллайдера арены)
        float spawnY = bounds.min.y + (blockSize.y / 2f);

        // Ограничиваем зону случайного выбора с учетом отступов от краев
        float minX = bounds.min.x + arenaEdgeOffset;
        float maxX = bounds.max.x - arenaEdgeOffset;
        float minZ = bounds.min.z + arenaEdgeOffset;
        float maxZ = bounds.max.z - arenaEdgeOffset;

        Vector3 potentialPos = bounds.center;
        potentialPos.y = spawnY;

        // Делаем до 100 попыток найти свободное место
        for (int attempt = 0; attempt < 100; attempt++)
        {
            float randX = Random.Range(minX, maxX);
            float randZ = Random.Range(minZ, maxZ);
            potentialPos = new Vector3(randX, spawnY, randZ);

            bool isTooClose = false;
            foreach (Vector3 pos in existingPositions)
            {
                if (Vector3.Distance(potentialPos, pos) < minDistanceBetweenTowers)
                {
                    isTooClose = true;
                    break;
                }
            }

            if (!isTooClose)
            {
                return potentialPos; // Нашли отличную точку!
            }
        }

        return potentialPos; // Вернем последнюю, если арена забита (крайний случай)
    }

    public GameObject SpawnJengaTower(Vector3 basePos, int index, BoxCollider activeZone, List<GameObject> targetList, string towerPrefix, string blockPrefix)
    {
        GameObject root = new GameObject(towerPrefix + index);
        targetList.Add(root);

        // Заранее выбираем, какие этажи будут наклонены (рандомный набор индексов)
        HashSet<int> tiltedFloors = GetTiltedFloors();

        float currentY = basePos.y; // Начинаем прямо с вычисленной высоты базы

        for (int y = 0; y < towerHeight; y++)
        {
            bool isLastFloor = (y == towerHeight - 1);
            bool isRotated = (y % 2 != 0);
            // Один случайный цвет на весь этаж из заданного набора
            Color floorColor = (floorColors != null && floorColors.Length > 0)
                ? floorColors[Random.Range(0, floorColors.Length)]
                : Random.ColorHSV(0f, 1f, 0.5f, 1f, 0.6f, 1f);
            // Рандомно решаем, что темнее на этом этаже: центральный блок или крайние
            bool darkenCenter = Random.value < 0.5f;
            float rowWidth = (blocksPerRow * blockSize.z) + ((blocksPerRow - 1) * gap);
            float startOffset = -rowWidth / 2f + (blockSize.z / 2f);
            Quaternion floorRotation = isRotated ? Quaternion.Euler(0, 90f, 0) : Quaternion.identity;

            // Случайный поворот вокруг оси Y для выбранных этажей, чтобы башня была не совсем прямая.
            // Поворачиваем весь слой как единое целое вокруг его центра, чтобы блоки не разъехались.
            Quaternion tiltRotation = Quaternion.identity;
            if (tiltedFloors.Contains(y))
            {
                float tiltAngle = Random.Range(minTiltAngle, maxTiltAngle);
                if (allowNegativeTilt && Random.value < 0.5f) tiltAngle = -tiltAngle;
                tiltRotation = Quaternion.Euler(0, tiltAngle, 0);
            }

            Vector3 floorCenter = basePos + new Vector3(0, currentY - basePos.y, 0);

            for (int i = 0; i < blocksPerRow; i++)
            {
                GameObject block = Instantiate(blockPrefab, root.transform);
                float localOffset = startOffset + i * (blockSize.z + gap);
                Vector3 positionOffset = new Vector3(0, 0, localOffset);
                float randomShift = Random.Range(-0.15f, 0.15f);
                Vector3 shiftOffset = new Vector3(randomShift, 0, 0);

                Vector3 finalPos = floorCenter + (tiltRotation * (floorRotation * (positionOffset + shiftOffset)));
                block.transform.SetPositionAndRotation(finalPos, tiltRotation * floorRotation);
                //block.transform.localScale = blockSize;

                if (isLastFloor) block.name = blockPrefix + index;
                else block.name = $"Block_{y}_{i}";

                if (block.TryGetComponent<BlockHealth>(out var health))
                {
                    health.destoyInWater = y > 1;
                }

                bool isCenterBlock = (i == blocksPerRow / 2);
                bool darken = darkenCenter ? isCenterBlock : !isCenterBlock;
                Color blockColor = darken ? DarkenColor(floorColor, darkenAmount) : floorColor;
                ApplyColor(block, blockColor);
            }
            currentY += blockSize.y;
        }
        return root;
    }

    // Выбираем случайный набор этажей, которые будут наклонены.
    // Последний этаж (с финальным блоком) не наклоняем.
    private HashSet<int> GetTiltedFloors()
    {
        HashSet<int> tilted = new HashSet<int>();

        int min = Mathf.Min(minTiltedFloors, maxTiltedFloors);
        int max = Mathf.Max(minTiltedFloors, maxTiltedFloors);
        int tiltCount = Random.Range(min, max + 1);

        // Кандидаты — все этажи кроме последнего
        List<int> candidates = new List<int>();
        for (int y = 0; y < towerHeight - 1; y++) candidates.Add(y);

        tiltCount = Mathf.Min(tiltCount, candidates.Count);

        for (int i = 0; i < tiltCount; i++)
        {
            int idx = Random.Range(0, candidates.Count);
            tilted.Add(candidates[idx]);
            candidates.RemoveAt(idx);
        }

        return tilted;
    }

    // Затемняет цвет на заданную долю (0.2 = на 20% темнее), не трогая альфу
    private Color DarkenColor(Color color, float amount)
    {
        float factor = 1f - Mathf.Clamp01(amount);
        return new Color(color.r * factor, color.g * factor, color.b * factor, color.a);
    }

    void ApplyColor(GameObject block, Color color)
    {
        Renderer rend = block.GetComponentInChildren<Renderer>();
        if (rend == null) return;
        MaterialPropertyBlock mpb = new();
        rend.GetPropertyBlock(mpb);
        mpb.SetColor("_Color", color);
        rend.SetPropertyBlock(mpb);
    }

    public void ClearSingleTower(int towerIndex)
    {
        string targetTowerName = "Tower_" + towerIndex;
        GameObject targetTower = GameObject.Find(targetTowerName);

        if (targetTower != null)
        {
            Destroy(targetTower);
            Debug.Log($"[BlocksSpawner] Башня {targetTowerName} успешно удалена со сцены.");
        }
    }

    public void Clear()
    {
        foreach (var t in mainTowers) if (t) Destroy(t);
        mainTowers.Clear();
    }

    public void ClearDuel()
    {
        foreach (var t in duelTowers) if (t) Destroy(t);
        duelTowers.Clear();
    }
}