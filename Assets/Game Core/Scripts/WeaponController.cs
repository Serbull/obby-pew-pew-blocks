using UnityEngine;

public class WeaponController : MonoBehaviour
{
    [Header("Shop Reference")]
    private ShopManager shopManager;

    [Header("Bullet Trail Settings")]
    public TrailRenderer bulletTrailPrefab; // Твой префаб линии из папки Assets
    public Transform muzzlePoint;          // Точка дула (скрипт пытается найти её сам)
    public float trailSpeed = 150f;        // Скорость полета полоски пули

    [Header("Shooting Settings")]
    public float fireRange = 100f;         // Максимальная дистанция выстрела

    void Start()
    {
        shopManager = FindFirstObjectByType<ShopManager>();
        UpdateWeaponVisibility();
    }

    void Update()
    {
        // Выстрел на Левую Кнопку Мыши
        if (Input.GetMouseButtonDown(0))
        {
            Shoot();
        }
    }

    // === ЛОГИКА ВЫСТРЕЛА С ТРЕЙЛОМ ===
    private void Shoot()
    {
        // ЖЕЛЕЗОБЕТОННЫЙ КОСТЫЛЬ: Если дуло не найдено, стреляем прямо из центра самого WeaponPrefab
        Transform activeFirePoint = (muzzlePoint != null) ? muzzlePoint : this.transform;

        // Пускаем луч строго вперед
        Ray ray = new Ray(activeFirePoint.position, activeFirePoint.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, fireRange))
        {
            Debug.Log($"[Выстрел] Попадание в: {hit.collider.name}");

            // Рисуем трейл от пушки до точки попадания
            SpawnTrail(activeFirePoint.position, hit.point);
        }
        else
        {
            // Если улетело в небо, пускаем трейл вперед на максимальную длину
            Vector3 targetPoint = ray.GetPoint(fireRange);
            SpawnTrail(activeFirePoint.position, targetPoint);
        }
    }

    // === СПАВН И ПОЛЕТ ЛИНИИ ПУЛИ ===
    public void SpawnTrail(Vector3 startPos, Vector3 hitPoint)
    {
        if (bulletTrailPrefab == null)
        {
            Debug.LogError("[WeaponController] Ошибка! В инспекторе в поле 'Bullet Trail Prefab' НЕ закинут префаб!");
            return;
        }

        // Спавним линию в точке выстрела
        TrailRenderer trail = Instantiate(bulletTrailPrefab, startPos, Quaternion.identity);
        StartCoroutine(MoveTrail(trail, hitPoint));
    }

    private System.Collections.IEnumerator MoveTrail(TrailRenderer trail, Vector3 hitPoint)
    {
        Vector3 startPosition = trail.transform.position;
        float distance = Vector3.Distance(startPosition, hitPoint);

        while (distance > 0.1f)
        {
            if (trail == null) break;

            // Плавно тащим линию к цели
            trail.transform.position = Vector3.MoveTowards(trail.transform.position, hitPoint, trailSpeed * Time.deltaTime);
            distance = Vector3.Distance(trail.transform.position, hitPoint);
            
            yield return null;
        }

        if (trail != null)
        {
            trail.transform.position = hitPoint;
            // Удаляем объект, когда время жизни хвоста выйдет (чтобы не забивать память)
            Destroy(trail.gameObject, trail.time);
        }
    }

    // === СМЕНА ПУШЕК И АВТОПОИСК ДУЛА ===
    public void UpdateWeaponVisibility()
    {
        if (shopManager == null) shopManager = FindFirstObjectByType<ShopManager>();
        if (shopManager == null || shopManager.allSkins == null) return;

        WeaponSkin equippedSkin = null;
        foreach (WeaponSkin s in shopManager.allSkins)
        {
            if (s != null && s.isEquipped)
            {
                equippedSkin = s;
                break;
            }
        }

        if (equippedSkin == null)
        {
            foreach (Transform child in transform) child.gameObject.SetActive(false);
            muzzlePoint = null;
            return;
        }

        for (int i = 0; i < shopManager.allSkins.Length; i++)
        {
            WeaponSkin currentSkin = shopManager.allSkins[i];
            if (currentSkin == null || currentSkin.weaponPrefab == null) continue;

            string targetWeaponName = currentSkin.weaponPrefab.name;
            Transform weaponChild = transform.Find(targetWeaponName);

            if (weaponChild == null && !string.IsNullOrEmpty(currentSkin.idInHand))
            {
                weaponChild = transform.Find(currentSkin.idInHand);
            }

            if (weaponChild != null)
            {
                if (currentSkin == equippedSkin)
                {
                    weaponChild.gameObject.SetActive(true);

                    // Глубокий поиск точки MuzzlePoint внутри включенного оружия
                    Transform foundMuzzle = null;
                    foreach (Transform child in weaponChild.GetComponentsInChildren<Transform>(true))
                    {
                        if (child.name == "MuzzlePoint")
                        {
                            foundMuzzle = child;
                            break;
                        }
                    }
                    
                    if (foundMuzzle != null)
                    {
                        muzzlePoint = foundMuzzle;
                    }
                    else
                    {
                        // Если точка не найдется, костыль в Shoot() всё равно спасет выстрел
                        muzzlePoint = null;
                    }
                }
                else
                {
                    weaponChild.gameObject.SetActive(false);
                }
            }
        }
    }
}