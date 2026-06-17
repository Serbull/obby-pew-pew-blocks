using UnityEngine;

public class ShopManager : MonoBehaviour
{
    [Header("Shop Settings")]
    public int coins = 1000;
    public WeaponSkin[] allSkins;
    public TMPro.TextMeshProUGUI mainCoinsText;

    [Header("UI")]
    public GameObject buttonPrefab;
    public Transform weaponGridContainer;   // Контейнер со ВСЕМИ пушками (попап «Пушки»)
    public WeaponInfoPanel infoPanel;       // Боковая панель с инфо о пушке

    void Awake()
    {
        LoadGameDataLocal();
        UpdateCoinsUI();
    }

    public void Start()
    {
        SpawnWeaponButtons();
        RefreshWeaponPositions();
    }

    // === ОТРИСОВКА КНОПОК ВСЕХ ПУШЕК ===
    public void SpawnWeaponButtons()
    {
        if (weaponGridContainer == null) return;

        foreach (Transform child in weaponGridContainer) { Destroy(child.gameObject); }

        foreach (WeaponSkin skin in allSkins)
        {
            if (skin == null) continue;

            GameObject newButton = Instantiate(buttonPrefab, weaponGridContainer);

            SkinButton skinButtonScript = newButton.GetComponent<SkinButton>();
            if (skinButtonScript != null)
            {
                skinButtonScript.Setup(skin, this);
            }
        }
    }

    // === КЛИК ПО КНОПКЕ ПУШКИ — ТОЛЬКО ПОКАЗ ИНФО СБОКУ ===
    // Подсветка кнопки тут не трогается: она отражает реально выбранную пушку
    // и обновляется при перерисовке кнопок (SpawnWeaponButtons) после экипировки.
    public void SelectSkin(WeaponSkin skin)
    {
        if (infoPanel != null) infoPanel.Show(skin);
    }

    // Показ инфо о текущей экипированной пушке (вызывается при открытии попапа)
    public void ShowEquippedInfo()
    {
        WeaponSkin equipped = null;
        if (allSkins != null)
        {
            foreach (WeaponSkin s in allSkins)
            {
                if (s != null && s.isEquipped) { equipped = s; break; }
            }
            if (equipped == null && allSkins.Length > 0) equipped = allSkins[0];
        }

        SelectSkin(equipped);
    }

    // === ПОКУПКА ПУШКИ (кнопка КУПИТЬ в боковой панели) ===
    public void BuySkin(WeaponSkin skin)
    {
        int skinIndex = IndexOf(skin);
        if (skinIndex == -1) return;

        WeaponSkin activeSkin = allSkins[skinIndex];
        if (activeSkin.isPurchased) return;

        if (coins < activeSkin.price)
        {
            Debug.LogWarning($"[Магазин] Не хватает монет! Баланс: {coins}, Нужно: {activeSkin.price}.");
            return;
        }

        coins -= activeSkin.price;
        activeSkin.isPurchased = true;

        // После покупки сразу экипируем купленную пушку
        for (int i = 0; i < allSkins.Length; i++) allSkins[i].isEquipped = (i == skinIndex);

        Debug.Log($"[Магазин] Успешная покупка! Списано: {activeSkin.price}. Остаток: {coins}");

        ApplyChangesAndRefresh(activeSkin);
    }

    // === ВЫБОР ПУШКИ (кнопка ВЫБРАТЬ в боковой панели) ===
    public void EquipSkin(WeaponSkin skin)
    {
        int skinIndex = IndexOf(skin);
        if (skinIndex == -1) return;
        if (!allSkins[skinIndex].isPurchased) return;

        for (int i = 0; i < allSkins.Length; i++) allSkins[i].isEquipped = (i == skinIndex);

        Debug.Log($"[Магазин] Скин {allSkins[skinIndex].skinName} успешно экипирован!");

        ApplyChangesAndRefresh(allSkins[skinIndex]);
    }

    // Сохраняем флаги, обновляем оружие в руке, монеты, кнопки и инфо
    private void ApplyChangesAndRefresh(WeaponSkin selected)
    {
        SaveGameData();
        RefreshWeaponPositions();
        UpdateCoinsUI();
        SpawnWeaponButtons();
        SelectSkin(selected);
    }

    private int IndexOf(WeaponSkin skin)
    {
        if (skin == null || allSkins == null) return -1;
        for (int i = 0; i < allSkins.Length; i++)
        {
            // Сравниваем по ссылке и по имени для 100% совпадения
            if (allSkins[i] == skin || allSkins[i].skinName == skin.skinName) return i;
        }
        Debug.LogError("Кликнутый скин оружия не найден в массиве allSkins в ShopManager!");
        return -1;
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
        var data = SaveManager.Data;
        data.coins = coins;

        EnsureSkinLists(data);
        for (int i = 0; i < allSkins.Length; i++)
        {
            data.skinsPurchased[i] = allSkins[i].isPurchased;
            data.skinsEquipped[i] = allSkins[i].isEquipped;
        }

        SaveManager.SaveGameData();
        Debug.Log($"[Сохранение] Прогресс записан! Баланс: {coins}.");
    }

    public void LoadGameDataLocal()
    {
        var data = SaveManager.Data;
        coins = (int)data.coins;

        bool hasSavedSkins = data.skinsPurchased.Count > 0;
        for (int i = 0; i < allSkins.Length; i++)
        {
            bool defaultActive = (i == 0);
            if (hasSavedSkins && i < data.skinsPurchased.Count)
            {
                allSkins[i].isPurchased = data.skinsPurchased[i];
                allSkins[i].isEquipped = data.skinsEquipped[i];
            }
            else
            {
                allSkins[i].isPurchased = defaultActive;
                allSkins[i].isEquipped = defaultActive;
            }
        }
    }

    private void EnsureSkinLists(YG.SavesYG data)
    {
        while (data.skinsPurchased.Count < allSkins.Length) data.skinsPurchased.Add(false);
        while (data.skinsEquipped.Count < allSkins.Length) data.skinsEquipped.Add(false);
    }

    [ContextMenu("Add 1000 Coins")]
    public void AddTestCoins()
    {
        coins += 1000;
        SaveGameData();
        UpdateCoinsUI();
        SpawnWeaponButtons();
    }

    [ContextMenu("Reset Progress")]
    public void ResetProgress()
    {
        var data = SaveManager.Data;
        data.coins = 0;
        data.skinsPurchased.Clear();
        data.skinsEquipped.Clear();
        SaveManager.SaveGameData();

        LoadGameDataLocal();
        UpdateCoinsUI();
        if (Application.isPlaying) SpawnWeaponButtons();
        RefreshWeaponPositions();
        Debug.Log("[Админ] Сохранения сброшены!");
    }
}
