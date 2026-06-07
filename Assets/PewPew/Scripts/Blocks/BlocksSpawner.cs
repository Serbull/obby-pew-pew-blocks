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

    public void SpawnJengaTower(Vector3 basePos, int index)
    {
        // Создаем корневой объект для башни
        GameObject root = new GameObject("Tower_" + index);
        towers.Add(root);

        // Начинаем спавн с самого низа арены
        float currentY = arenaZone.bounds.min.y + (blockSize.y / 2f);

        // Идем снизу вверх по этажам
        for (int y = 0; y < towerHeight; y++)
        {
            bool isLastFloor = (y == towerHeight - 1);
            // Каждый четный этаж повернут на 90 градусов (классическая Дженга)
            bool isRotated = (y % 2 != 0);

            // Генерируем случайный цвет для всего этажа
            Color floorColor = Random.ColorHSV(0f, 1f, 0.5f, 1f, 0.6f, 1f);

            // Считаем общую ширину ряда, чтобы центрировать блоки
            float rowWidth = (blocksPerRow * blockSize.z) + ((blocksPerRow - 1) * gap);
            float startOffset = -rowWidth / 2f + (blockSize.z / 2f);

            // Базовый поворот этажа (строго 0 или 90 градусов, никаких кривых углов!)
            Quaternion floorRotation = isRotated ? Quaternion.Euler(0, 90f, 0) : Quaternion.identity;

            for (int i = 0; i < blocksPerRow; i++)
            {
                // Спавним блок
                GameObject block = Instantiate(blockPrefab, root.transform);

                // Расчет позиции блока в ряду
                float localOffset = startOffset + i * (blockSize.z + gap);
                Vector3 positionOffset = new Vector3(0, 0, localOffset);

                // --- ЭФФЕКТ СДВИГА ИЗ ОРИГИНАЛА ---
                // Случайно смещаем блок ВПЕРЕД или НАЗАД вдоль его длинной части (ось X)
                // Значение 0.15f означает разброс до 15 сантиметров. Башня не упадет, но будет выглядеть хаотично.
                float randomShift = Random.Range(-0.15f, 0.15f);
                Vector3 shiftOffset = new Vector3(randomShift, 0, 0);

                // Финальная позиция с учетом шахматного поворота башни
                Vector3 finalPos = basePos + new Vector3(0, currentY - basePos.y, 0) + (floorRotation * (positionOffset + shiftOffset));

                block.transform.position = finalPos;
                block.transform.rotation = floorRotation;
                block.transform.localScale = blockSize;

                // Если это самый верхний этаж, маркируем блоки для GameManager
                if (isLastFloor)
                {
                    block.name = "FinalBlock_" + index;
                }
                else
                {
                    block.name = $"Block_{y}_{i}";
                }

                ApplyColor(block, floorColor);
            }

            // Поднимаемся на высоту одного блока для следующего этажа
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

    public void Clear()
    {
        foreach (var t in towers) if (t) Destroy(t);
        towers.Clear();
    }

    // void Start() => SpawnTowers();
}