using System.Collections.Generic;
using UnityEngine;

public class BotShooter : MonoBehaviour
{
	private Weapon weapon;
	private PlayerWeaponEquip weaponEquip;
	private Animator anim;

	[Header("Настройки ИИ")]
	public float minShootDelay = 1.5f;
	public float maxShootDelay = 4.0f;
	[Tooltip("Радиус разброса бота. Чем больше, тем сильнее он косит мимо блоков")]
	public float inaccuracyRadius = 0.7f;

	[HideInInspector] public bool isDuelBot = false;
	private GameObject playerObject;

	private List<GameObject> enemyBlocks = new List<GameObject>();
	private float nextShootTime;
	private int myTowerIndex = -1;

	public void InitializeBot(int towerIndex)
	{
		myTowerIndex = towerIndex;
		anim = GetComponent<Animator>();
		if (anim == null) anim = GetComponentInChildren<Animator>();

		weaponEquip = GetComponent<PlayerWeaponEquip>();
		playerObject = GameObject.FindGameObjectWithTag("Player");

		if (weaponEquip != null)
		{
			weaponEquip.EquipWeapon();
		}

		nextShootTime = Time.time + Random.Range(minShootDelay, maxShootDelay);
		UpdateEnemyBlocksList();
	}

	void Update()
	{
		if (myTowerIndex == -1 || weaponEquip == null) return;

		if (weapon == null)
		{
			weapon = weaponEquip.GetWeapon();
			return;
		}

		bool isEquipped = (weapon.transform.parent == weaponEquip.handPoint);

		if (isEquipped)
		{
			anim.SetBool("IsAiming", true);

			if (Time.time >= nextShootTime)
			{
				ShootAtRandomBlock();
				nextShootTime = Time.time + Random.Range(minShootDelay, maxShootDelay);
			}
		}
		else
		{
			anim.SetBool("IsAiming", false);
		}
	}

	private void ShootAtRandomBlock()
	{
		enemyBlocks.RemoveAll(item => item == null);

		if (enemyBlocks.Count == 0)
		{
			UpdateEnemyBlocksList();
		}

		if (enemyBlocks.Count == 0)
		{
			if (isDuelBot && playerObject != null)
			{
				ShootDirectlyAtPlayer();
			}
			return;
		}

		GameObject targetBlock = enemyBlocks[Random.Range(0, enemyBlocks.Count)];
		if (targetBlock == null) return;

		Vector3 targetPoint = targetBlock.transform.position + new Vector3(
			Random.Range(-inaccuracyRadius, inaccuracyRadius),
			Random.Range(-inaccuracyRadius, inaccuracyRadius),
			Random.Range(-inaccuracyRadius, inaccuracyRadius)
		);

		Vector3 lookDirection = targetBlock.transform.position - transform.position;
		lookDirection.y = 0;
		if (lookDirection != Vector3.zero)
		{
			transform.rotation = Quaternion.LookRotation(lookDirection);
		}

		if (weapon != null)
		{
			weapon.ShootBot(targetPoint);
		}
	}

	private void ShootDirectlyAtPlayer()
	{
		Vector3 targetPoint = playerObject.transform.position + new Vector3(
			Random.Range(-inaccuracyRadius, inaccuracyRadius),
			Random.Range(-inaccuracyRadius, inaccuracyRadius),
			Random.Range(-inaccuracyRadius, inaccuracyRadius)
		);

		Vector3 lookDirection = playerObject.transform.position - transform.position;
		lookDirection.y = 0;
		if (lookDirection != Vector3.zero)
		{
			transform.rotation = Quaternion.LookRotation(lookDirection);
		}

		if (weapon != null)
		{
			weapon.ShootBot(targetPoint);
		}
	}

	private void UpdateEnemyBlocksList()
	{
		enemyBlocks.Clear();

		if (isDuelBot)
		{
			GameObject towerRoot = GameObject.Find("Duel_Tower_0");
			if (towerRoot != null)
			{
				foreach (Transform child in towerRoot.transform)
				{
					enemyBlocks.Add(child.gameObject);
				}
			}
		}
		else
		{
			// ИСПРАВЛЕНИЕ: Вместо жесткого цикла до 4, мы ищем ВСЕ существующие башни "Tower_X" на сцене динамически!
			// Находим все корневые объекты, чьи имена начинаются с "Tower_"
			GameObject[] allObjects = GameObject.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
			foreach (GameObject go in allObjects)
			{
				if (go.name.StartsWith("Tower_"))
				{
					// Вычленяем индекс башни из её имени (например "Tower_5" -> 5)
					if (int.TryParse(go.name.Replace("Tower_", ""), out int towerIdx))
					{
						// Свою собственную башню не обстреливаем
						if (towerIdx == myTowerIndex) continue;

						foreach (Transform child in go.transform)
						{
							enemyBlocks.Add(child.gameObject);
						}
					}
				}
			}
		}
	}
}