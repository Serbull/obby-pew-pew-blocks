using UnityEngine;

public class WeaponController : MonoBehaviour
{
    [Header("Shop Reference")]
    private ShopManager shopManager;

    // ДОБАВИЛИ: Проверка, принадлежит ли этот контроллер боту
    private bool isBot = false;

    void Start()
    {
        // Проверяем, есть ли у этого персонажа (или его родителя) тег Бота или скрипт управления ботом
        if (transform.root.CompareTag("Bot") || transform.root.name.Contains("Bot"))
        {
            isBot = true;
        }

        // Если это бот — ему магазин не нужен, у него всегда дефолтная пушка
        if (isBot) return;

        shopManager = FindFirstObjectByType<ShopManager>();
        UpdateWeaponVisibility();
    }

    public void UpdateWeaponVisibility()
    {
        // Если это бот — мгновенно выходим, ничего не прячем и не ломаем
        if (isBot) return;

        if (shopManager == null) shopManager = FindFirstObjectByType<ShopManager>();
        if (shopManager == null || shopManager.allSkins == null) return;

        // 1. Сначала выключаем ВСЁ оружие в руках у ИГРОКА
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(false);
        }

        // 2. Ищем экипированный в магазине скин
        WeaponSkin equippedSkin = null;
        foreach (WeaponSkin s in shopManager.allSkins)
        {
            if (s != null && s.isEquipped)
            {
                equippedSkin = s;
                break;
            }
        }

        if (equippedSkin == null) return;

        // 3. Ищем нужную пушку по имени префаба или по idInHand
        string targetWeaponName = equippedSkin.weaponPrefab != null ? equippedSkin.weaponPrefab.name : "";
        Transform weaponChild = transform.Find(targetWeaponName);

        if (weaponChild == null && !string.IsNullOrEmpty(equippedSkin.idInHand))
        {
            weaponChild = transform.Find(equippedSkin.idInHand);
        }

        // 4. Включаем её игроку
        if (weaponChild != null)
        {
            weaponChild.gameObject.SetActive(true);
            Debug.Log($"[WeaponController] Игроку включена пушка: {weaponChild.name}");
        }
        else
        {
            Debug.LogError($"[WeaponController] Не найден дочерний объект оружия '{targetWeaponName}' или '{equippedSkin.idInHand}' внутри Игрока!");
        }
    }
}