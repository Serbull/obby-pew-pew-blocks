using UnityEngine;

public class BulletTracer : MonoBehaviour
{
	public float speed = 10f;
	public int damage = 1;
	public float lifeTime = 3f;

	private Vector3 direction;

	public void Init(Vector3 dir)
	{
		direction = dir.normalized;
		transform.forward = direction;
		Destroy(gameObject, lifeTime);
	}

	void Update()
	{
		transform.position += direction * speed * Time.deltaTime;
	}

	void OnTriggerEnter(Collider other)
	{
		if (other.isTrigger) return;

		IDamageable dmg = other.GetComponent<IDamageable>();
		if (dmg != null)
		{
			// Находим точную точку на поверхности коллайдера блока, куда прилетела пуля
			Vector3 hitPoint = other.ClosestPoint(transform.position);

			// Передаем все данные
			dmg.TakeDamage(damage, direction, hitPoint);
		}

		Destroy(gameObject);
	}
}