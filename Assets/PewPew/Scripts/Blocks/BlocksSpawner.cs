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

    private List<GameObject> mainTowers = new List<GameObject>();
    private List<GameObject> duelTowers = new List<GameObject>();

    public List<GameObject> SpawnTowers(int count = 4, BoxCollider targetZone = null)
    {
        BoxCollider activeZone = targetZone != null ? targetZone : arenaZone;
        Bounds b = activeZone.bounds;
        List<GameObject> createdTowers = new List<GameObject>();

        if (count == 4)
        {
            Clear();
            float offset = b.size.x * 0.25f;
            Vector3[] bases = {
                b.center + new Vector3(-offset, 0, -offset),
                b.center + new Vector3( offset, 0, -offset),
                b.center + new Vector3(-offset, 0,  offset),
                b.center + new Vector3( offset, 0,  offset)
            };

            for (int i = 0; i < 4; i++)
            {
                // Для лобби оставляем имя "Tower_"
                GameObject tower = SpawnJengaTower(bases[i], i, activeZone, mainTowers, "Tower_", "FinalBlock_");
                createdTowers.Add(tower);
            }
        }
        else if (count == 2)
        {
            ClearDuel();
            float offset = b.size.x * 0.25f;
            Vector3[] bases = {
                b.center + new Vector3(-offset, 0, 0),
                b.center + new Vector3( offset, 0, 0)
            };

            for (int i = 0; i < 2; i++)
            {
                // ИЗМЕНЕНИЕ: Для дуэли даем уникальные префиксы "Duel_Tower_" и "Duel_FinalBlock_"
                GameObject tower = SpawnJengaTower(bases[i], i, activeZone, duelTowers, "Duel_Tower_", "Duel_FinalBlock_");
                createdTowers.Add(tower);
            }
        }

        return createdTowers;
    }

    // Добавили префиксы имен в параметры метода
    public GameObject SpawnJengaTower(Vector3 basePos, int index, BoxCollider activeZone, List<GameObject> targetList, string towerPrefix, string blockPrefix)
    {
        GameObject root = new GameObject(towerPrefix + index);
        targetList.Add(root);

        float currentY = activeZone.bounds.min.y + (blockSize.y / 2f);

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

                // Используем переданный префикс блока
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