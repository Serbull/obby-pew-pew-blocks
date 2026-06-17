using UnityEngine;

public class BulletTracer : MonoBehaviour
{
	public float speed = 10f;
	public int damage = 1;
	public float lifeTime = 7.5f;

	private float forceMultiplier = 1f; // Множитель силы толчка блока (берётся из скина пушки)
	private Vector3 direction;
	private bool isHit = false; // Защита от двойного просчета попадания

	// Ссылки на компоненты для красивого скрытия
	private Collider bulletCollider;
	private MeshRenderer bulletRenderer;
	private TrailRenderer bulletTrail;

	void Awake()
	{
		// Кэшируем компоненты при спавне пули
		bulletCollider = GetComponent<Collider>();
		bulletRenderer = GetComponent<MeshRenderer>();
		bulletTrail = GetComponent<TrailRenderer>();
	}

	public void Init(Vector3 dir)
	{
		direction = dir.normalized;
		transform.forward = direction;

		// Перестраховка: если пуля улетит в пустоту, она удалится сама
		Destroy(gameObject, lifeTime);
	}

	// Перегрузка с параметрами из скина пушки: скорость пули и множитель силы толчка блока
	public void Init(Vector3 dir, float bulletSpeed, float force)
	{
		speed = bulletSpeed;
		forceMultiplier = force;
		Init(dir);
	}

	void Update()
	{
		// Если пуля уже попала, мы её больше не двигаем
		if (isHit) return;

		transform.position += direction * speed * Time.deltaTime;
	}

	void OnTriggerEnter(Collider other)
	{
		if (other.isTrigger || isHit) return;
		isHit = true; // Фиксируем удар

		IDamageable dmg = other.GetComponent<IDamageable>();
		if (dmg != null)
		{
			// Находим точную точку на поверхности коллайдера блока, куда прилетела пуля
			Vector3 hitPoint = other.ClosestPoint(transform.position);

			// Передаем все данные
			dmg.TakeDamage(damage, direction, hitPoint, forceMultiplier);
		}

		// Вместо резкого Destroy запускаем красивое исчезновение хвоста
		HandleBulletDestruction();
	}

	private void HandleBulletDestruction()
	{
		// Отключаем триггер, чтобы пуля больше ни с чем не взаимодействовала
		if (bulletCollider != null) bulletCollider.enabled = false;

		// Прячем саму модельку пули (красную штучку)
		if (bulletRenderer != null) bulletRenderer.enabled = false;

		// Узнаем, сколько времени живет твой след в Trail Renderer (например, 0.15 сек)
		float trailDuration = bulletTrail != null ? bulletTrail.time : 0.1f;

		// Удаляем объект пули ТОЛЬКО после того, как догорит её хвост
		Destroy(gameObject, trailDuration);
	}
}