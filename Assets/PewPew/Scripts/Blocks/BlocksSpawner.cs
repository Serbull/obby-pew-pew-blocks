using UnityEngine;
using System.Collections.Generic;

public class TowerSpawner : MonoBehaviour
{
    public GameObject blockPrefab;
    public BoxCollider arenaZone;

    [Header("Tower settings")]
    public int towerHeight = 15;

    [Header("Block size")]
    public Vector2 xSize = new Vector2(1f, 5f);
    public Vector2 ySize = new Vector2(0.6f, 1f);
    public Vector2 zSize = new Vector2(3f, 5f);

    private List<GameObject> blocks = new List<GameObject>();

    public void SpawnTowers()
    {
        Clear();

        Bounds b = arenaZone.bounds;

        Vector3 center = b.center;

        float halfX = b.size.x * 0.25f;
        float halfZ = b.size.z * 0.25f;

        Vector3[] bases =
        {
            center + new Vector3(-halfX, 0, -halfZ),
            center + new Vector3( halfX, 0, -halfZ),
            center + new Vector3(-halfX, 0,  halfZ),
            center + new Vector3( halfX, 0,  halfZ)
        };

        for (int i = 0; i < 4; i++)
            SpawnTower(bases[i]);
    }

	void ApplyColor(GameObject block)
{
    Renderer rend = block.GetComponent<Renderer>();
    if (rend == null) return;

    MaterialPropertyBlock mpb = new MaterialPropertyBlock();
    rend.GetPropertyBlock(mpb);

    mpb.SetColor("_Color", new Color(
        Random.Range(0.3f, 1f),
        Random.Range(0.3f, 1f),
        Random.Range(0.3f, 1f)
    ));

    rend.SetPropertyBlock(mpb);
}

    void SpawnTower(Vector3 basePos)
    {
        float currentY = arenaZone.bounds.min.y;

        for (int i = 0; i < towerHeight; i++)
        {
            float sx = Random.Range(xSize.x, xSize.y);
            float sy = Random.Range(ySize.x, ySize.y);
            float sz = Random.Range(zSize.x, zSize.y);

            GameObject block = Instantiate(blockPrefab, transform);

            block.transform.localScale = new Vector3(sx, sy, sz);

            currentY += sy / 2f;

            block.transform.position = new Vector3(
                basePos.x,
                currentY,
                basePos.z
            );

            currentY += sy / 2f;

			ApplyColor(block);

            blocks.Add(block);
        }
    }

    void Clear()
    {
        foreach (var b in blocks)
            Destroy(b);

        blocks.Clear();
    }
		void Start()
{
    SpawnTowers();
}
}