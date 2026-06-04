using UnityEngine;

public class ShopManager : MonoBehaviour
{
    [Header("Shop Settings")]
    public int coins = 1000;                     
    public WeaponSkin[] allSkins;               
    public TMPro.TextMeshProUGUI mainCoinsText; 

    [Header("UI Containers")]
    public GameObject buttonPrefab;             
    public Transform shopGridContainer;         
    public Transform inventoryGridContainer;    

    void Awake()
    {
        // 1. Сразу загружаем сохранения при старте игры
        LoadGameDataLocal(); 
        UpdateCoinsUI();
    }

    public void Start()
    {
        // Принудительно подтягиваем актуальное инфо из памяти при старте сцены
        LoadGameDataLocal();
        UpdateCoinsUI();

        // 2. Создаем кнопки в магазине и инвентаре
        SpawnShopButtons();
        RefreshWeaponPositions(); 
    }

    // === ОТРИСОВКА КНОПОК ===
    public void SpawnShopButtons()
    {
        // Очищаем старые плашки перед перерисовкой
        if (shopGridContainer != null)
        {
            foreach (Transform child in shopGridContainer) { Destroy(child.gameObject); }
        }
        
        if (inventoryGridContainer != null)
        {
            foreach (Transform child in inventoryGridContainer) { Destroy(child.gameObject); }
        }

        // Создаем новые плашки под каждую пушку
        for (int i = 0; i < allSkins.Length; i++)
        {
            WeaponSkin skin = allSkins[i];

            // Если куплено — отправляем в инвентарь (GUNS), если нет — в магазин (SHOP)
            Transform targetContainer = skin.isPurchased ? inventoryGridContainer : shopGridContainer;

            if (targetContainer == null) continue;

            GameObject newButton = Instantiate(buttonPrefab, targetContainer);

            SkinButton skinButtonScript = newButton.GetComponent<SkinButton>();
            if (skinButtonScript != null)
            {
                skinButtonScript.Setup(skin, this);
            }
        }
    }

    // === ЛОГИКА КЛИКА ПО СКИНУ (ПОКУПКА / ЭКИПИРОВКА) ===
    public void OnClickSkin(WeaponSkin skin)
    {
        // 1. Жесткая синхронизация: перед любой операцией берем точный баланс из памяти!
        LoadGameDataLocal();

        // 2. Находим индекс текущего скина в нашем общем массиве магазина
        int skinIndex = -1;
        for (int i = 0; i < allSkins.Length; i++)
        {
            if (allSkins[i] == skin)
            {
                skinIndex = i;
                break;
            }
        }

        // Если скин вдруг не найден в массиве
        if (skinIndex == -1)
        {
            Debug.LogError("Кликнутый скин оружия не найден в массиве allSkins в ShopManager!");
            return;
        }

        // Работаем строго через элемент массива по индексу
        WeaponSkin activeSkin = allSkins[skinIndex];

        if (activeSkin.isPurchased)
        {
            // Если скин уже куплен — экипируем его, а со всех остальных снимаем экипировку
            for (int i = 0; i < allSkins.Length; i++)
            {
                allSkins[i].isEquipped = (i == skinIndex);
            }
            Debug.Log($"[Магазин] Скин {activeSkin.skinName} успешно экипирован!");
        }
        else
        {
            // Если скин еще не куплен — проверяем цену именно этого оружия
            if (coins >= activeSkin.price)
            {
                coins -= activeSkin.price;
                activeSkin.isPurchased = true;

                // Сразу автоматически экипируем только что купленный скин
                for (int i = 0; i < allSkins.Length; i++)
                {
                    allSkins[i].isEquipped = (i == skinIndex);
                }
                
                Debug.Log($"[Магазин] Успешная покупка! Списано: {activeSkin.price}. Остаток на счету: {coins}");
            }
            else
            {
                Debug.LogWarning($"[Магазин] Не хватает монет! Баланс: {coins}, Нужно: {activeSkin.price}. Скин: {activeSkin.skinName}");
                return; 
            }
        }

        // 3. Обновляем всё на сцене и сохраняем прогресс
        RefreshWeaponPositions(); 
        SaveGameData();      // Сохраняем измененный баланс и статусы пушек в PlayerPrefs
        UpdateCoinsUI();     // Обновляем текст баланса на экране
        SpawnShopButtons();  // Перерисовываем кнопки (переносим купленное в инвентарь)
    }

    public void RefreshWeaponPositions()
    {
        WeaponController handWeaponController = FindFirstObjectByType<WeaponController>();
        if (handWeaponController != null)
        {
            handWeaponController.UpdateWeaponVisibility();
        }
    }

    public void UpdateCoinsUI()
    {
        if (mainCoinsText != null) mainCoinsText.text = coins.ToString();
    }

    // === СОХРАНЕНИЕ ===
    public void SaveGameData()
    {
        PlayerPrefs.SetInt("Coins", coins);

        for (int i = 0; i < allSkins.Length; i++)
        {
            PlayerPrefs.SetInt("Skin_Purchased_" + i, allSkins[i].isPurchased ? 1 : 0);
            PlayerPrefs.SetInt("Skin_Equipped_" + i, allSkins[i].isEquipped ? 1 : 0);
        }
        PlayerPrefs.Save();
        Debug.Log($"[Сохранение] Прогресс записан! Баланс: {coins}.");
    }

    public void LoadGameDataLocal()
    {
        coins = PlayerPrefs.GetInt("Coins", 0);
        
        for (int i = 0; i < allSkins.Length; i++)
        {
            // Первый скин (индекс 0) по умолчанию куплен и экипирован, остальные закрыты
            int defaultActive = (i == 0) ? 1 : 0;
            allSkins[i].isPurchased = PlayerPrefs.GetInt("Skin_Purchased_" + i, defaultActive) == 1;
            allSkins[i].isEquipped = PlayerPrefs.GetInt("Skin_Equipped_" + i, defaultActive) == 1;
        }
    }

    // === ТЕСТОВЫЕ КНОПКИ ДЛЯ ИНСПЕКТОРА (БЕЗ ОШИБОК HEADER) ===
    [ContextMenu("Add 1000 Coins")]
    public void AddTestCoins()
    {
        LoadGameDataLocal();
        coins += 1000;
        SaveGameData();
        UpdateCoinsUI();
        SpawnShopButtons();
        Debug.Log("[Админ] Начислено +1000 тестовых монет!");
    }

    [ContextMenu("Reset Progress")]
    public void ResetProgress()
    {
        PlayerPrefs.DeleteAll();
        LoadGameDataLocal();
        UpdateCoinsUI();
        if (Application.isPlaying) SpawnShopButtons();
        Debug.Log("[Админ] Все сохранения сброшены!");
    }
}