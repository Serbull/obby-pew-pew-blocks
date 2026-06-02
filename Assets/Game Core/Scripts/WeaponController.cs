using UnityEngine;

public class WeaponController : MonoBehaviour
{
    [Header("Shop Reference")]
    private ShopManager shopManager;

    void Start()
    {
        // Автоматически находим скрипт магазина на сцене
        shopManager = FindFirstObjectByType<ShopManager>();

        // Сразу при старте обновляем пушку в руке
        UpdateWeaponVisibility();
    }

    // Этот метод мы будем вызывать из магазина каждый раз, когда меняем скин
    public void UpdateWeaponVisibility()
    {
        if (shopManager == null) shopManager = FindFirstObjectByType<ShopManager>();
        if (shopManager == null || shopManager.allSkins == null) return;

        // 1. Ищем, какой скин сейчас выбран (equipped) в магазине
        WeaponSkin equippedSkin = null;
        foreach (WeaponSkin s in shopManager.allSkins)
        {
            if (s != null && s.isEquipped)
            {
                equippedSkin = s;
                break;
            }
        }

        // 2. Пробегаемся по всем пушкам, которые лежат внутри этого WaponPrefab
        for (int i = 0; i < shopManager.allSkins.Length; i++)
        {
            if (shopManager.allSkins[i] == null || shopManager.allSkins[i].weaponPrefab == null) continue;

            // Берем точное имя префаба пушки из настроек магазина
            string targetWeaponName = shopManager.allSkins[i].weaponPrefab.name;

            // Ищем объект с таким именем прямо внутри себя (в детях WaponPrefab)
            Transform weaponChild = transform.Find(targetWeaponName);

            if (weaponChild != null)
            {
                // Если это выбранная пушка — включаем её, остальные — гасим
                if (shopManager.allSkins[i] == equippedSkin)
                {
                    weaponChild.gameObject.SetActive(true);
                }
                else
                {
                    weaponChild.gameObject.SetActive(false);
                }
            }
        }
    }
}
