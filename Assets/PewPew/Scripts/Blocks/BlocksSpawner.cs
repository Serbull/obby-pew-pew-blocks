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
    [Range(0, 10)]
    public int minTiltedFloors = 2;
    [Range(0, 10)]
    public int maxTiltedFloors = 4;
    public float minTiltAngle = 30f;
    public float maxTiltAngle = 45f;
    [Tooltip("Если включено, этаж может повернуться и в минус (влево)")]
    public bool allowNegativeTilt = true;

    private List<GameObject> towers = new List<GameObject>();

    public void SpawnTowers()
    {
        Clear();
        Bounds b = arenaZone.bounds;
        float offset = b.size.x * 0.25f;

        Vector3[] bases = {
            b.center + new Vector3(-offset, 0, -offset),
            b.center + new Vector3( offset, 0, -offset),
            b.center + new Vector3(-offset, 0,  offset),
            b.center + new Vector3( offset, 0,  offset)
        };

        for (int i = 0; i < 4; i++)
            SpawnJengaTower(bases[i], i);
    }

    void SpawnJengaTower(Vector3 basePos, int index)
    {
        GameObject root = new GameObject("Tower_" + index);
        towers.Add(root);

        // Настройки рандома этажей (без изменений)
        int tiltedCount = Random.Range(minTiltedFloors, maxTiltedFloors + 1);
        HashSet<int> tiltedFloors = new HashSet<int>();
        int safetyNet = 0;
        while (tiltedFloors.Count < tiltedCount && safetyNet < 100)
        {
            int randY = Random.Range(1, towerHeight - 1);
            tiltedFloors.Add(randY);
            safetyNet++;
        }

        float currentY = arenaZone.bounds.min.y + (blockSize.y / 2f);

        for (int y = 0; y < towerHeight; y++)
        {
            bool isLastFloor = (y == towerHeight - 1);
            bool isRotated = (y % 2 != 0);
            Color floorColor = Random.ColorHSV(0f, 1f, 0.5f, 1f, 0.6f, 1f);

            float rowWidth = (blocksPerRow * blockSize.z) + ((blocksPerRow - 1) * gap);
            float startOffset = -rowWidth / 2f + (blockSize.z / 2f);

            float floorAngle = isRotated ? 90f : 0f;
            if (tiltedFloors.Contains(y))
            {
                float angle = Random.Range(minTiltAngle, maxTiltAngle);
                if (allowNegativeTilt && Random.value > 0.5f) angle *= -1f;
                floorAngle = angle;
            }

            if (isLastFloor)
            {
                // --- ЛОГИКА СПАЯННОЙ ПЛАТФОРМЫ ---
                GameObject platformRoot = new GameObject("FinalPlatform");
                platformRoot.transform.SetParent(root.transform);
                platformRoot.transform.position = basePos + new Vector3(0, currentY - basePos.y + 0.05f, 0);
                platformRoot.transform.rotation = Quaternion.Euler(0, floorAngle, 0);

                // Добавляем физику на всю платформу целиком
                Rigidbody platformRb = platformRoot.AddComponent<Rigidbody>();
                platformRb.isKinematic = true;
                platformRb.useGravity = false;

                // Добавляем скрипт ХП (чтобы платформа тоже могла ломаться/активироваться)
                BlockHealth platformHealth = platformRoot.AddComponent<BlockHealth>();
                platformHealth.towerRoot = root.transform;
                platformHealth.health = 10; // Платформа прочнее

                for (int d = 0; d < 2; d++)
                {
                    float totalDepth = (2 * blockSize.z) + gap;
                    float startDepthOffset = -totalDepth / 2f + (blockSize.z / 2f);
                    float depthOffset = startDepthOffset + d * (blockSize.z + gap);

                    for (int i = 0; i < 2; i++)
                    {
                        GameObject block = Instantiate(blockPrefab, platformRoot.transform);
                        float localOffset = (-((2 * blockSize.z) + gap) / 2f + (blockSize.z / 2f)) + i * (blockSize.z + gap);

                        // Позиция относительно родителя (platformRoot)
                        block.transform.localPosition = new Vector3(depthOffset, 0, localOffset);
                        block.transform.localRotation = Quaternion.identity;
                        block.transform.localScale = blockSize;

                        // УДАЛЯЕМ лишние компоненты, чтобы они не конфликтовали с родителем
                        if (block.TryGetComponent<Rigidbody>(out var rb)) Destroy(rb);
                        if (block.TryGetComponent<BlockHealth>(out var bh)) Destroy(bh);

                        ApplyColor(block, floorColor);
                    }
                }
            }
            else
            {
                // --- ОБЫЧНЫЙ СПАВН ЭТАЖЕЙ ---
                for (int i = 0; i < blocksPerRow; i++)
                {
                    GameObject block = Instantiate(blockPrefab, root.transform);
                    float localOffset = startOffset + i * (blockSize.z + gap);
                    Vector3 positionOffset = new Vector3(0, 0, localOffset);
                    Quaternion rotation = Quaternion.Euler(0, floorAngle, 0);

                    block.transform.position = basePos + new Vector3(0, currentY - basePos.y, 0) + (rotation * positionOffset);
                    block.transform.rotation = rotation;
                    block.transform.localScale = blockSize;

                    ApplyColor(block, floorColor);
                    if (block.TryGetComponent<BlockHealth>(out var bh)) bh.towerRoot = root.transform;
                }
            }
            currentY += blockSize.y;
        }
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

    void Clear()
    {
        foreach (var t in towers) if (t) Destroy(t);
        towers.Clear();
    }

    void Start() => SpawnTowers();
}