using System.Collections.Generic;
using UnityEngine;

public class BotShooter : MonoBehaviour
{
	private Weapon weapon;
	private PlayerWeaponEquip weaponEquip; // Используем твой же скрипт экипировки!
	private Animator anim;

	[Header("Настройки ИИ")]
	public float minShootDelay = 1.5f;
	public float maxShootDelay = 4.0f;
	[Tooltip("Радиус разброса бота. Чем больше, тем сильнее он косит мимо блоков")]
	public float inaccuracyRadius = 0.7f;

	private List<GameObject> enemyBlocks = new List<GameObject>();
	private float nextShootTime;
	private int myTowerIndex = -1;

	// Метод вызывается из GameController, когда бот заспавнился на своей башне
	public void InitializeBot(int towerIndex)
	{
		myTowerIndex = towerIndex;
		anim = GetComponent<Animator>();
		if (anim == null) anim = GetComponentInChildren<Animator>();

		weaponEquip = GetComponent<PlayerWeaponEquip>();

		// Сразу надеваем пушку в руки
		if (weaponEquip != null)
		{
			weaponEquip.EquipWeapon();
		}

		nextShootTime = Time.time + Random.Range(minShootDelay, maxShootDelay);

		// Находим все блоки вражеских башен
		UpdateEnemyBlocksList();
	}

	void Update()
	{
		if (myTowerIndex == -1 || weaponEquip == null) return;

		// Пытаемся получить оружие, если оно еще не инициализировалось
		if (weapon == null)
		{
			weapon = weaponEquip.GetWeapon();
			return;
		}

		// Проверяем, в руке ли оружие (по твоей логике)
		bool isEquipped = (weapon.transform.parent == weaponEquip.handPoint);

		if (isEquipped)
		{
			anim.SetBool("IsAiming", true);

			// Логика таймера стрельбы
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
		// Удаляем из списка блоки, которые уже уничтожены пулями
		enemyBlocks.RemoveAll(item => item == null);

		// ПОДСТРАХОВКА: Если блоки кончились, пробуем обновить список 
		// (вдруг какая-то башня еще цела, а мы её пропустили)
		if (enemyBlocks.Count == 0)
		{
			UpdateEnemyBlocksList();
		}

		if (enemyBlocks.Count == 0) return;

		// Выбираем случайный кирпич чужой башни
		GameObject targetBlock = enemyBlocks[Random.Range(0, enemyBlocks.Count)];
		if (targetBlock == null) return;

		// Считаем точку с учетом косоглазия бота
		Vector3 targetPoint = targetBlock.transform.position + new Vector3(
			Random.Range(-inaccuracyRadius, inaccuracyRadius),
			Random.Range(-inaccuracyRadius, inaccuracyRadius),
			Random.Range(-inaccuracyRadius, inaccuracyRadius)
		);

		// Поворачиваем бота лицом к блоку
		Vector3 lookDirection = targetBlock.transform.position - transform.position;
		lookDirection.y = 0;
		if (lookDirection != Vector3.zero)
		{
			transform.rotation = Quaternion.LookRotation(lookDirection);
		}

		// Выстрел
		if (weapon != null)
		{
			weapon.ShootBot(targetPoint);
		}
	}

	private void UpdateEnemyBlocksList()
	{
		enemyBlocks.Clear();

		for (int i = 0; i < 4; i++)
		{
			if (i == myTowerIndex) continue; // Свою башню бот игнорирует

			GameObject towerRoot = GameObject.Find("Tower_" + i);
			if (towerRoot != null)
			{
				foreach (Transform child in towerRoot.transform)
				{
					enemyBlocks.Add(child.gameObject);
				}
			}
		}
	}
}