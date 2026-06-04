using UnityEngine;

public class WeaponController : MonoBehaviour
{
    [Header("Shop Reference")]
    private ShopManager shopManager;

    void Start()
    {
        shopManager = FindFirstObjectByType<ShopManager>();
        UpdateWeaponVisibility();
    }

    public void UpdateWeaponVisibility()
    {
        if (shopManager == null) shopManager = FindFirstObjectByType<ShopManager>();
        if (shopManager == null || shopManager.allSkins == null) return;

        // 1. Ищем, какой скин сейчас экипирован
        WeaponSkin equippedSkin = null;
        foreach (WeaponSkin s in shopManager.allSkins)
        {
            if (s != null && s.isEquipped)
            {
                equippedSkin = s;
                break;
            }
        }

        // Если ничего не экипировано, выключаем вообще всё оружие в руках
        if (equippedSkin == null)
        {
            foreach (Transform child in transform) child.gameObject.SetActive(false);
            return;
        }

        // 2. Проходимся по всем скинам из магазина
        for (int i = 0; i < shopManager.allSkins.Length; i++)
        {
            WeaponSkin currentSkin = shopManager.allSkins[i];
            if (currentSkin == null || currentSkin.weaponPrefab == null) continue;

            string targetWeaponName = currentSkin.weaponPrefab.name;
            Transform weaponChild = transform.Find(targetWeaponName);

            // ЗАЩИТА ОТ ОПЕЧАТОК: Если по имени префаба не нашли, пробуем искать по ID из Scriptable Object
            if (weaponChild == null && !string.IsNullOrEmpty(currentSkin.idInHand))
            {
                weaponChild = transform.Find(currentSkin.idInHand);
            }

            // Если объект в руке наконец-то найден
            if (weaponChild != null)
            {
                if (currentSkin == equippedSkin)
                {
                    weaponChild.gameObject.SetActive(true);
                    Debug.Log($"[Оружие] Включили визуализацию для: {weaponChild.name}");
                }
                else
                {
                    weaponChild.gameObject.SetActive(false);
                }
            }
            else
            {
                // Если пушка так и не нашлась — выдаем жесткий варнинг в консоль, чтобы сразу видеть косяк
                if (currentSkin == equippedSkin)
                {
                    Debug.LogError($"[WeaponController] Хьюстон, проблема! Игрок выбрал {currentSkin.skinName}, но в объекте {gameObject.name} нет дочернего объекта с именем '{targetWeaponName}' или '{currentSkin.idInHand}'!");
                }
            }
        }
    }
}