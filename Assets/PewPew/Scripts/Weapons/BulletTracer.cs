using UnityEngine;

public class BulletTracer : MonoBehaviour
{
	public float speed = 120f;
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
		// 🔥 ВОТ ЭТОГО НЕ ХВАТАЛО
		transform.position += direction * speed * Time.deltaTime;
	}

	void OnTriggerEnter(Collider other)
	{
		IDamageable dmg = other.GetComponent<IDamageable>();
		if (dmg != null)
		{
			dmg.TakeDamage(damage);
		}

		Destroy(gameObject);
	}
}