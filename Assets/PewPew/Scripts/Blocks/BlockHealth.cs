using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BlockHealth : MonoBehaviour, IDamageable
{
    public int health = 3;
    public float destroyDelay = 1.5f;
    public float wakeRadius = 15f;

    private Rigidbody rb;
    private bool destroyed = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;
    }

    public void TakeDamage(int damage)
    {
        if (destroyed) return;

        health -= damage;

        if (health <= 0)
            BreakBlock();
    }

    void BreakBlock()
    {
        destroyed = true;

        ActivatePhysics();

        Collider[] hits = Physics.OverlapSphere(transform.position, wakeRadius);

        foreach (Collider hit in hits)
        {
            BlockHealth other = hit.GetComponent<BlockHealth>();
            if (other != null && !other.destroyed)
            {
                other.ActivatePhysics();
            }
        }

        Destroy(gameObject, destroyDelay);
    }

    public void ActivatePhysics()
    {
        if (!rb.isKinematic) return;

        rb.isKinematic = false;
        rb.useGravity = true;
        rb.AddForce(Random.insideUnitSphere * 2f, ForceMode.Impulse);
    }
}