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
        LoadGameDataLocal();
        UpdateCoinsUI();
    }

    public void Start()
    {
        SpawnShopButtons();
        RefreshWeaponPositions();
    }

    // === ОТРИСОВКА КНОПОК ===
    public void SpawnShopButtons()
    {
        if (shopGridContainer != null)
        {
            foreach (Transform child in shopGridContainer) { Destroy(child.gameObject); }
        }

        if (inventoryGridContainer != null)
        {
            foreach (Transform child in inventoryGridContainer) { Destroy(child.gameObject); }
        }

        for (int i = 0; i < allSkins.Length; i++)
        {
            WeaponSkin skin = allSkins[i];
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
        // ИСПРАВЛЕНО: Убран вызов LoadGameDataLocal() в начале, который затирал текущий клик!

        int skinIndex = -1;
        for (int i = 0; i < allSkins.Length; i++)
        {
            // Сравниваем не только ссылки, но и имена для 100% гарантии совпадения
            if (allSkins[i] == skin || allSkins[i].skinName == skin.skinName)
            {
                skinIndex = i;
                break;
            }
        }

        if (skinIndex == -1)
        {
            Debug.LogError("Кликнутый скин оружия не найден в массиве allSkins в ShopManager!");
            return;
        }

        WeaponSkin activeSkin = allSkins[skinIndex];

        if (activeSkin.isPurchased)
        {
            for (int i = 0; i < allSkins.Length; i++)
            {
                allSkins[i].isEquipped = (i == skinIndex);
            }
            Debug.Log($"[Магазин] Скин {activeSkin.skinName} успешно экипирован!");
        }
        else
        {
            if (coins >= activeSkin.price)
            {
                coins -= activeSkin.price;
                activeSkin.isPurchased = true;

                for (int i = 0; i < allSkins.Length; i++)
                {
                    allSkins[i].isEquipped = (i == skinIndex);
                }

                Debug.Log($"[Магазин] Успешная покупка! Списано: {activeSkin.price}. Остаток: {coins}");
            }
            else
            {
                Debug.LogWarning($"[Магазин] Не хватает монет! Баланс: {coins}, Нужно: {activeSkin.price}.");
                return;
            }
        }

        // ИСПРАВЛЕНО: Сначала сохраняем изменённые флаги пушек в память
        SaveGameData();

        // Теперь обновляем визуал (контроллер считает уже новые сохранённые флаги)
        RefreshWeaponPositions();

        UpdateCoinsUI();
        SpawnShopButtons();
    }

    public void RefreshWeaponPositions()
    {
        // Находим живого игрока по тегу, чтобы случайно не обновить бота
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            // Ищем контроллер оружия именно внутри игрока (в его руках)
            WeaponController handWeaponController = playerObj.GetComponentInChildren<WeaponController>();
            if (handWeaponController != null)
            {
                handWeaponController.UpdateWeaponVisibility();
                Debug.Log("[Магазин] Визуал оружия обновлен строго для Игрока.");
            }
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
            int defaultActive = (i == 0) ? 1 : 0;
            allSkins[i].isPurchased = PlayerPrefs.GetInt("Skin_Purchased_" + i, defaultActive) == 1;
            allSkins[i].isEquipped = PlayerPrefs.GetInt("Skin_Equipped_" + i, defaultActive) == 1;
        }
    }

    [ContextMenu("Add 1000 Coins")]
    public void AddTestCoins()
    {
        coins += 1000;
        SaveGameData();
        UpdateCoinsUI();
        SpawnShopButtons();
    }

    [ContextMenu("Reset Progress")]
    public void ResetProgress()
    {
        PlayerPrefs.DeleteAll();
        LoadGameDataLocal();
        UpdateCoinsUI();
        if (Application.isPlaying) SpawnShopButtons();
        RefreshWeaponPositions();
        Debug.Log("[Админ] Сохранения сброшены!");
    }
}