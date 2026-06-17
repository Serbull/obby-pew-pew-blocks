using UnityEngine;

public class WeaponController : MonoBehaviour
{
    [Header("Shop Reference")]
    private ShopManager shopManager;

    // ДОБАВИЛИ: Проверка, принадлежит ли этот контроллер боту
    private bool isBot = false;

    // Текущий экипированный скин (у бота остаётся null — используются дефолтные параметры пули)
    public WeaponSkin EquippedSkin { get; private set; }

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

        // Ищем экипированный в магазине скин
        WeaponSkin equippedSkin = null;
        foreach (WeaponSkin s in shopManager.allSkins)
        {
            if (s != null && s.isEquipped)
            {
                equippedSkin = s;
                break;
            }
        }

        ApplySkin(equippedSkin);
    }

    // Выдаёт боту случайную пушку из всех доступных скинов магазина.
    // Вызывается из BotShooter при инициализации бота.
    public void EquipRandomSkin()
    {
        if (shopManager == null) shopManager = FindFirstObjectByType<ShopManager>();
        if (shopManager == null || shopManager.allSkins == null || shopManager.allSkins.Length == 0) return;

        // Собираем только валидные скины, чтобы не выпал null
        var validSkins = new System.Collections.Generic.List<WeaponSkin>();
        foreach (WeaponSkin s in shopManager.allSkins)
        {
            if (s != null) validSkins.Add(s);
        }

        if (validSkins.Count == 0) return;

        WeaponSkin randomSkin = validSkins[Random.Range(0, validSkins.Count)];
        ApplySkin(randomSkin);
        Debug.Log($"[WeaponController] Боту выдана случайная пушка: {randomSkin.skinName}");
    }

    // Общая логика: прячет всё оружие и включает визуал нужного скина, запоминая его параметры пули.
    private void ApplySkin(WeaponSkin skin)
    {
        // 1. Сначала выключаем ВСЁ оружие в руках
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(false);
        }

        // Запоминаем скин, чтобы Weapon мог взять его параметры (скорость/сила пули)
        EquippedSkin = skin;

        if (skin == null) return;

        // 2. Ищем нужную пушку по имени префаба или по idInHand
        string targetWeaponName = skin.weaponPrefab != null ? skin.weaponPrefab.name : "";
        Transform weaponChild = transform.Find(targetWeaponName);

        if (weaponChild == null && !string.IsNullOrEmpty(skin.idInHand))
        {
            weaponChild = transform.Find(skin.idInHand);
        }

        // 3. Включаем её
        if (weaponChild != null)
        {
            weaponChild.gameObject.SetActive(true);
            Debug.Log($"[WeaponController] Включена пушка: {weaponChild.name}");
        }
        else
        {
            Debug.LogError($"[WeaponController] Не найден дочерний объект оружия '{targetWeaponName}' или '{skin.idInHand}'!");
        }
    }
}