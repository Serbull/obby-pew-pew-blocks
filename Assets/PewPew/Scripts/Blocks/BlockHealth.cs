using UnityEngine;
using System.Collections;

public class BlockHealth : MonoBehaviour, IDamageable
{
    [Tooltip("Сила выбивания блока. Подбирай в районе 50 - 500 для тяжелых блоков")]
    public float pushForce = 300f;

    public bool destoyInWater = false;

    private bool isInWater = false;
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        // ВАЖНО: Физика включена СРАЗУ. Блоки просто стоят друг на друге под силой тяжести.
        rb.isKinematic = false;
        rb.useGravity = true;

        // Помогает избежать проваливания блоков друг в друга при сильных ударах
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
    }

    // Принимаем урон, направление, ТОЧКУ попадания пули и множитель силы от пушки
    public void TakeDamage(int damage, Vector3 dir, Vector3 hitPoint, float forceMultiplier = 1f)
    {
        if (rb != null)
        {
            // Прикладываем силу в конкретную точку попадания, умноженную на силу конкретной пушки.
            // Это заставит блок не просто лететь вперед, но и реалистично закручиваться, передавая импульс соседям.
            rb.AddForceAtPosition(dir * pushForce * forceMultiplier, hitPoint, ForceMode.Impulse);
        }
    }

    // Логика воды (Water)
    private void OnTriggerEnter(Collider other)
    {
        // Первый ряд всегда стоит в воде, поэтому он не должен исчезать
        if (!destoyInWater) return;

        if (!isInWater && other.GetComponent<WaterDeath>() != null)
        {
            isInWater = true;
            StartCoroutine(DestroyAfterDelay(3f));
        }
    }

    private IEnumerator DestroyAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        float timer = 0f;
        Vector3 originalScale = transform.localScale;
        while (timer < 1f)
        {
            timer += Time.deltaTime * 2f; // Исчезновение за 0.5 секунды
            transform.localScale = Vector3.Lerp(originalScale, Vector3.zero, timer);
            yield return null;
        }

        Destroy(gameObject);
    }
}