using UnityEngine;

public class ShopManager : MonoBehaviour
{
    [Header("Shop Settings")]
    public int coins = 1000;                    // Твой баланс монет
    public WeaponSkin[] allSkins;               // Список всех пушек
    public TMPro.TextMeshProUGUI mainCoinsText; // Текст монет на главном экране

    [Header("UI Containers")]
    public GameObject buttonPrefab;             // Наш шаблон SkinButtonPrefab
    public Transform shopGridContainer;         // Объект Grid из Магазина
    public Transform inventoryGridContainer;    // Объект Grid из Инвентаря

    [Header("Weapon Points")]
    public Transform weaponHandPoint;           // Точка в руке игрока
    public Transform weaponBackPoint;           // Точка за спиной игрока

    private GameObject currentHandWeapon;       // Текущая пушка в руке
    private GameObject currentBackWeapon;       // Текущая пушка за спиной

    void Awake()
    {
        LoadGameData(); // Загружаем монеты и пушки из памяти при старте
        UpdateCoinsUI();
    }

    void Start()
    {
        SpawnShopButtons();
        RefreshWeaponPositions(); // Расставляем пушки при старте
    }

    public void SpawnShopButtons()
    {
        // Очищаем старые кнопки в магазине
        foreach (Transform child in shopGridContainer)
        {
            Destroy(child.gameObject);
        }
        // Очищаем старые кнопки в инвентаре
        foreach (Transform child in inventoryGridContainer)
        {
            Destroy(child.gameObject);
        }

        // Спавним кнопки для Магазина
        foreach (WeaponSkin skin in allSkins)
        {
            GameObject shopBtn = Instantiate(buttonPrefab, shopGridContainer);
            SkinButton btnScript = shopBtn.GetComponent<SkinButton>();

            if (btnScript != null)
            {
                btnScript.skinData = skin;
                btnScript.shopManager = this;
                btnScript.isShopButton = true; // Это кнопка магазина
                btnScript.RenderButton();
            }
        }

        // Спавним кнопки для Инвентаря (только купленные)
        foreach (WeaponSkin skin in allSkins)
        {
            if (skin.isPurchased)
            {
                GameObject invBtn = Instantiate(buttonPrefab, inventoryGridContainer);
                SkinButton btnScript = invBtn.GetComponent<SkinButton>();

                if (btnScript != null)
                {
                    btnScript.skinData = skin;
                    btnScript.shopManager = this;
                    btnScript.isShopButton = false; // Это кнопка инвентаря
                    btnScript.RenderButton();
                }
            }
        }
    }

    public void OnClickSkin(WeaponSkin skin)
    {
        if (skin.isPurchased)
        {
            // Если уже куплено — экипируем/надеваем
            foreach (WeaponSkin s in allSkins)
            {
                if (s == skin) s.isEquipped = true;
                else if (s.isEquipped) s.isEquipped = false; // Снимаем с остальных
            }
        }
        else
        {
            // Если не куплено — пытаемся купить
            if (coins >= skin.price)
            {
                coins -= skin.price;
                skin.isPurchased = true;

                // Сразу автоматически надеваем после покупки
                foreach (WeaponSkin s in allSkins)
                {
                    if (s == skin) s.isEquipped = true;
                    else if (s.isEquipped) s.isEquipped = false;
                }
            }
            else
            {
                Debug.Log("Не хватает монет!");
                return; // Выходим, если денег нет
            }
        }

        // Полностью обновляем позиции пушек, текст монет, сохраняем прогресс и перерисовываем кнопки
        RefreshWeaponPositions();
        UpdateCoinsUI();
        SaveGameData(); // Сохраняем всё на диск!
        SpawnShopButtons();
    }

    public void RefreshWeaponPositions()
    {
        // Удаляем старые пушки визуально
        if (currentHandWeapon != null) Destroy(currentHandWeapon);
        if (currentBackWeapon != null) Destroy(currentBackWeapon);

        WeaponSkin equippedSkin = null;
        WeaponSkin backSkin = null;

        // Ищем, какая пушка надета, а какая пойдет на спину
        foreach (WeaponSkin s in allSkins)
        {
            if (s.isEquipped)
            {
                equippedSkin = s;
            }
            else if (s.isPurchased && backSkin == null)
            {
                backSkin = s; // На спину вешаем первую попавшуюся купленную, но не надетую пушку
            }
        }

        // Спавним пушку в руку
        if (equippedSkin != null && equippedSkin.weaponPrefab != null && weaponHandPoint != null)
        {
            currentHandWeapon = Instantiate(equippedSkin.weaponPrefab, weaponHandPoint);
            currentHandWeapon.transform.localPosition = Vector3.zero;
            currentHandWeapon.transform.localRotation = Quaternion.identity;
        }

        // Спавним пушку на спину
        if (backSkin != null && backSkin.weaponPrefab != null && weaponBackPoint != null)
        {
            currentBackWeapon = Instantiate(backSkin.weaponPrefab, weaponBackPoint);
            currentBackWeapon.transform.localPosition = Vector3.zero;
            currentBackWeapon.transform.localRotation = Quaternion.identity;
        }
    }

    public void UpdateCoinsUI()
    {
        if (mainCoinsText != null)
        {
            mainCoinsText.text = coins.ToString();
        }
    }

    // === ЛОГИКА СОХРАНЕНИЯ И ЗАГРУЗКИ ===

    public void SaveGameData()
    {
        // Сохраняем монеты
        PlayerPrefs.SetInt("PlayerCoins", coins);

        // Сохраняем состояние каждой пушки по её индексу в массиве
        for (int i = 0; i < allSkins.Length; i++)
        {
            int purchased = allSkins[i].isPurchased ? 1 : 0;
            int equipped = allSkins[i].isEquipped ? 1 : 0;

            PlayerPrefs.SetInt("Skin_Purchased_" + i, purchased);
            PlayerPrefs.SetInt("Skin_Equipped_" + i, equipped);
        }

        PlayerPrefs.Save();
        Debug.Log("Прогресс успешно сохранен на диск!");
    }

    public void LoadGameData()
    {
        // Загружаем монеты. Если ключа нет (первый запуск), выдаст стартовые 1000 монет
        coins = PlayerPrefs.GetInt("PlayerCoins", 1000);

        // Загружаем пушки
        for (int i = 0; i < allSkins.Length; i++)
        {
            // По умолчанию для самой первой пушки в списке (индекс 0 - пистолет) ставим true, чтобы она была открыта всегда
            int defaultPurchased = (i == 0) ? 1 : 0;
            int defaultEquipped = (i == 0) ? 1 : 0;

            int purchased = PlayerPrefs.GetInt("Skin_Purchased_" + i, defaultPurchased);
            int equipped = PlayerPrefs.GetInt("Skin_Equipped_" + i, defaultEquipped);

            allSkins[i].isPurchased = (purchased == 1);
            allSkins[i].isEquipped = (equipped == 1);
        }

        Debug.Log("Прогресс успешно загружен из памяти!");
    }

    // Сброс сохранений (можно вызвать, нажав правой кнопкой на компонент скрипта в инспекторе)
    [ContextMenu("Reset Progress")]
    public void ResetProgress()
    {
        PlayerPrefs.DeleteAll();
        Debug.Log("Сохранения стерты! Перезапусти игру, чтобы проверить чистый баланс.");
    }
}