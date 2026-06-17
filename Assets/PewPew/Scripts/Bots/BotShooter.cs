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

	[Header("Настройки самосохранения")]
	[Range(0f, 1f)]
	[Tooltip("Вероятность того, что бот попытается спастись, когда блок под ним выбивают. 0 — бот тупой и всегда падает, 1 — всегда пытается убежать на устойчивый блок")]
	public float saveSelfChance = 0.5f;
	[Tooltip("Скорость опорного блока (м/с), выше которой бот считает, что блок выбивают и под ним вот-вот не будет опоры")]
	public float instabilityVelocity = 2.0f;
	[Tooltip("Горизонтальная скорость опорного блока (м/с), выше которой считается, что блок уползает вбок и бот вот-вот соскользнёт. Меньше instabilityVelocity, чтобы ловить медленное скольжение")]
	public float slideVelocity = 0.5f;
	[Tooltip("Скорость блока (м/с), ниже которой блок считается устойчивым и пригодным, чтобы на него убежать")]
	public float stableVelocity = 0.6f;
	[Tooltip("На сколько метров выше ног бота может быть блок, чтобы бот ещё мог на него запрыгнуть")]
	public float maxClimbHeight = 2.0f;
	[Tooltip("На сколько метров ниже ног бота допустимо спрыгивать на устойчивый блок")]
	public float maxDropHeight = 8.0f;
	[Tooltip("Сколько секунд под ботом должна быть твёрдая опора, прежде чем он перестанет спасаться. Гистерезис, чтобы бота не дёргало туда-сюда")]
	public float stableConfirmTime = 0.3f;

	[HideInInspector] public bool isDuelBot = false;
	private GameObject playerObject;

	private List<GameObject> enemyBlocks = new List<GameObject>();
	private float nextShootTime;
	private int myTowerIndex = -1;

	// --- Самосохранение ---
	private CharacterCore characterCore;
	private Transform ownTowerRoot;
	private bool dangerActive = false;   // блок под ботом сейчас нестабилен
	private bool willReact = false;      // решение (бросок кубика) на текущий эпизод опасности
	private bool isEscaping = false;     // бот сейчас бежит к устойчивому блоку
	private bool supportIsSliding = false; // опорный блок уползает вбок (нужно спрыгивать, а не просто бежать)
	private GameObject escapeTargetBlock;
	private float nextEscapeJumpTime;
	private float groundedStableSince = -1f; // момент, с которого под ботом снова твёрдая опора (для гистерезиса)

	public void InitializeBot(int towerIndex)
	{
		myTowerIndex = towerIndex;
		anim = GetComponent<Animator>();
		if (anim == null) anim = GetComponentInChildren<Animator>();

		characterCore = GetComponent<CharacterCore>();

		weaponEquip = GetComponent<PlayerWeaponEquip>();
		playerObject = GameObject.FindGameObjectWithTag("Player");

		if (weaponEquip != null)
		{
			weaponEquip.EquipWeapon();

			// Выдаём боту случайную пушку (визуал + параметры скорости/силы пули)
			Weapon botWeapon = weaponEquip.GetWeapon();
			if (botWeapon != null)
			{
				WeaponController botWeaponController = botWeapon.GetComponent<WeaponController>();
				if (botWeaponController != null)
				{
					botWeaponController.EquipRandomSkin();
				}
			}
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

		// Сначала проверяем, не выбивают ли блок из-под бота — самосохранение важнее стрельбы
		UpdateSelfPreservation();

		// Пока бот спасается (бежит/прыгает на устойчивый блок) — ему не до стрельбы
		if (isEscaping)
		{
			anim.SetBool("IsAiming", false);
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

	// --- ЛОГИКА САМОСОХРАНЕНИЯ ---
	// Если блок под ботом выбивают/он падает, бот с вероятностью saveSelfChance
	// пытается перебежать/перепрыгнуть на устойчивый блок своей башни, чтобы не упасть.
	private void UpdateSelfPreservation()
	{
		if (characterCore == null) return;

		bool hasSupport = HasStableGround(out Rigidbody supportRb);

		if (hasSupport)
		{
			if (isEscaping)
			{
				// Гистерезис: не прекращаем побег при первом же касании опоры, иначе бота
				// дёргает (опора то есть, то нет на грани порогов). Ждём, пока опора будет
				// стабильной несколько кадров подряд.
				if (groundedStableSince < 0f) groundedStableSince = Time.time;
				if (Time.time - groundedStableSince >= stableConfirmTime)
				{
					dangerActive = false;
					StopEscaping();
				}
			}
			else
			{
				dangerActive = false;
			}
			return;
		}

		// Опора потеряна или уезжает — сбрасываем таймер подтверждения стабильности
		groundedStableSince = -1f;

		// Кидаем "кубик" один раз на эпизод опасности.
		if (!dangerActive)
		{
			dangerActive = true;
			willReact = Random.value < saveSelfChance;
		}

		// Этот бот решил не реагировать в этот раз — пусть падает (остаётся "тупым")
		if (!willReact) return;

		// Цель выбираем ОДИН раз и держимся за неё, пока она не пропала или её саму не выбило.
		// Иначе каждый кадр выбирается новый "ближайший" блок и бот дёргается между ними.
		if (!IsTargetUsable(escapeTargetBlock))
		{
			escapeTargetBlock = FindNearestStableBlock(supportRb);
		}

		if (escapeTargetBlock == null) return; // бежать некуда

		isEscaping = true;
		MoveTowardTarget(escapeTargetBlock);
	}

	// Есть ли под ногами бота устойчивая опора?
	private bool HasStableGround(out Rigidbody supportRb)
	{
		supportRb = null;
		supportIsSliding = false;

		Vector3 origin = transform.position + Vector3.up * 1.0f;
		if (!Physics.SphereCast(origin, 0.3f, Vector3.down, out RaycastHit hit, 1.3f,
				characterCore.groundLayers, QueryTriggerInteraction.Ignore))
		{
			return false; // опоры нет вовсе — бот в воздухе/блок улетел
		}

		supportRb = hit.collider.GetComponentInParent<Rigidbody>();
		if (supportRb != null)
		{
			Vector3 v = supportRb.linearVelocity;

			// Блок выбивают — он летит/падает быстро
			if (v.magnitude > instabilityVelocity) return false;

			// Блок медленно уползает вбок — бот стоит на месте, "прилипает" к нему и вот-вот соскользнёт.
			// Ловим это раньше, чтобы бот успел спрыгнуть, а не падал вместе с блоком.
			float horizontalSpeed = new Vector2(v.x, v.z).magnitude;
			if (horizontalSpeed > slideVelocity)
			{
				supportIsSliding = true;
				return false;
			}
		}

		return true;
	}

	// Цель ещё годится, пока блок существует и его самого не выбивают.
	// Порог здесь намеренно мягкий (instabilityVelocity), чтобы не менять цель из-за лёгкого дрожания.
	private bool IsTargetUsable(GameObject block)
	{
		if (block == null) return false;
		Rigidbody rb = block.GetComponent<Rigidbody>();
		if (rb == null) return false;
		return rb.linearVelocity.magnitude <= instabilityVelocity;
	}

	// Ищем ближайший устойчивый блок в собственной башне бота, на который реально запрыгнуть.
	private GameObject FindNearestStableBlock(Rigidbody currentSupport)
	{
		Transform root = GetOwnTowerRoot();
		if (root == null) return null;

		float feetY = transform.position.y;
		GameObject best = null;
		float bestDist = float.MaxValue;

		foreach (Transform child in root)
		{
			if (currentSupport != null && child == currentSupport.transform) continue;

			Rigidbody rb = child.GetComponent<Rigidbody>();
			if (rb == null) continue;
			if (rb.linearVelocity.magnitude > stableVelocity) continue; // блок шатается/летит

			Collider col = child.GetComponentInChildren<Collider>();
			if (col == null) continue;

			float topY = col.bounds.max.y;
			if (topY > feetY + maxClimbHeight) continue; // слишком высоко, не запрыгнуть
			if (topY < feetY - maxDropHeight) continue;  // слишком низко, разобьётся

			Vector3 flat = child.position - transform.position;
			flat.y = 0f;
			float dist = flat.magnitude;
			if (dist < bestDist)
			{
				bestDist = dist;
				best = child.gameObject;
			}
		}

		return best;
	}

	private Transform GetOwnTowerRoot()
	{
		if (ownTowerRoot != null) return ownTowerRoot;

		string rootName = isDuelBot ? "Duel_Tower_" + myTowerIndex : "Tower_" + myTowerIndex;
		GameObject go = GameObject.Find(rootName);
		if (go != null) ownTowerRoot = go.transform;
		return ownTowerRoot;
	}

	private void MoveTowardTarget(GameObject target)
	{
		Vector3 dir = target.transform.position - transform.position;
		dir.y = 0f;

		if (dir.sqrMagnitude > 0.04f)
		{
			Vector3 n = dir.normalized;
			characterCore.moveAxis = n;

			// Поворачиваемся в сторону бега, чтобы анимация и проверка препятствий работали корректно
			Quaternion look = Quaternion.LookRotation(n);
			transform.rotation = Quaternion.Slerp(transform.rotation, look, 12f * Time.deltaTime);

			// Прыгаем, если: цель выше ног, впереди стенка из блоков, либо опорный блок уползает
			// вбок (нужно оттолкнуться и спрыгнуть, иначе бот просто соскользнёт вместе с ним).
			float targetTop = GetBlockTop(target);
			bool needClimb = targetTop > transform.position.y + 0.4f
				|| characterCore.SomethingInFront()
				|| supportIsSliding;
			if (needClimb && Time.time >= nextEscapeJumpTime)
			{
				characterCore.Jump();
				nextEscapeJumpTime = Time.time + 0.4f;
			}
		}
		else
		{
			characterCore.moveAxis = Vector3.zero;
		}
	}

	private float GetBlockTop(GameObject block)
	{
		Collider col = block.GetComponentInChildren<Collider>();
		return col != null ? col.bounds.max.y : block.transform.position.y;
	}

	private void StopEscaping()
	{
		isEscaping = false;
		escapeTargetBlock = null;
		if (characterCore != null) characterCore.moveAxis = Vector3.zero;
		anim.SetFloat("Move", 0);
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