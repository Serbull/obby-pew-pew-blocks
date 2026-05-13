using UnityEngine;

public class BlockHealth : MonoBehaviour, IDamageable
{
    public int health = 3;
    [HideInInspector] public Transform towerRoot;
    private bool destroyed = false;
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        // На старте жестко выключаем физику, чтобы башня не поплыла
        rb.isKinematic = true;
        rb.useGravity = false;
    }

    public void TakeDamage(int damage)
    {
        if (destroyed) return;
        health -= damage;
        if (health <= 0) BreakBlock();
    }

    void BreakBlock()
    {
        if (destroyed) return;
        destroyed = true;

        // Будим всех соседей по башне
        if (towerRoot != null)
        {
            BlockHealth[] allBlocks = towerRoot.GetComponentsInChildren<BlockHealth>();
            foreach (var b in allBlocks)
            {
                b.ActivatePhysics();
            }
        }

        // Выключаем коллизии сразу, чтобы не мешать падению остальных
        GetComponent<Collider>().enabled = false;
        GetComponent<Renderer>().enabled = false;
        Destroy(gameObject, 0.1f);
    }

    public void ActivatePhysics()
    {
        if (destroyed || rb == null) return;

        if (rb.isKinematic)
        {
            rb.isKinematic = false;
            rb.useGravity = true;

            // Уменьшаем вероятность "взрыва": 
            // CollisionDetectionMode.Continuous помогает лучше считать столкновения
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;

            // Если башня все равно взрывается, убери AddForce совсем или сделай его 0.01f
            rb.AddForce(Random.insideUnitSphere * 0.01f, ForceMode.Impulse);
        }
    }
}