using UnityEngine;

public interface IDamageable
{
    void TakeDamage(int damage, Vector3 dir, Vector3 hitPoint, float forceMultiplier = 1f);
}