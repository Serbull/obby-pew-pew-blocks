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

    [Header("Random Tilt Settings")]
    [Range(0, 10)] public int minTiltedFloors = 2;
    [Range(0, 10)] public int maxTiltedFloors = 4;
    public float minTiltAngle = 30f;
    public float maxTiltAngle = 45f;
    public bool allowNegativeTilt = true;

    [Header("Настройки Динамического Спавна")]
    public float minDistanceBetweenTowers = 12f; // Минимальное расстояние между башнями, чтобы не слипались
    public float arenaEdgeOffset = 4f;            // Отступ от краев зоны, чтобы башни не застревали в стенах

    private List<GameObject> mainTowers = new List<GameObject>();
    private List<GameObject> duelTowers = new List<GameObject>();

    // count теперь задается динамически из GameController (от 4 до 8)
    public List<GameObject> SpawnTowers(int count = 4, BoxCollider targetZone = null)
    {
        BoxCollider activeZone = targetZone != null ? targetZone : arenaZone;
        Bounds b = activeZone.bounds;
        List<GameObject> createdTowers = new List<GameObject>();


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

        float currentY = basePos.y; // Начинаем прямо с вычисленной высоты базы

        for (int y = 0; y < towerHeight; y++)
        {
            bool isLastFloor = (y == towerHeight - 1);
            bool isRotated = (y % 2 != 0);
            Color floorColor = Random.ColorHSV(0f, 1f, 0.5f, 1f, 0.6f, 1f);
            float rowWidth = (blocksPerRow * blockSize.z) + ((blocksPerRow - 1) * gap);
            float startOffset = -rowWidth / 2f + (blockSize.z / 2f);
            Quaternion floorRotation = isRotated ? Quaternion.Euler(0, 90f, 0) : Quaternion.identity;

            for (int i = 0; i < blocksPerRow; i++)
            {
                GameObject block = Instantiate(blockPrefab, root.transform);
                float localOffset = startOffset + i * (blockSize.z + gap);
                Vector3 positionOffset = new Vector3(0, 0, localOffset);
                float randomShift = Random.Range(-0.15f, 0.15f);
                Vector3 shiftOffset = new Vector3(randomShift, 0, 0);

                Vector3 finalPos = basePos + new Vector3(0, currentY - basePos.y, 0) + (floorRotation * (positionOffset + shiftOffset));
                block.transform.position = finalPos;
                block.transform.rotation = floorRotation;
                block.transform.localScale = blockSize;

                if (isLastFloor) block.name = blockPrefix + index;
                else block.name = $"Block_{y}_{i}";

                ApplyColor(block, floorColor);
            }
            currentY += blockSize.y;
        }
        return root;
    }

    void ApplyColor(GameObject block, Color color)
    {
        Renderer rend = block.GetComponent<Renderer>();
        if (rend == null) return;
        MaterialPropertyBlock mpb = new MaterialPropertyBlock();
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