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
        // 1. Сразу загружаем локальные сохранения компьютера
        LoadGameDataLocal(); 
        UpdateCoinsUI();
    }

    void Start()
    {
        // 2. ЖЕЛЕЗНО создаем кнопки при старте, чтобы магазин не был пустым
        SpawnShopButtons();
        RefreshWeaponPositions(); 
    }

    // === ОТРИСОВКА КНОПОК ===
    public void SpawnShopButtons()
    {
        // Очищаем старые плашки, чтобы они не дублировались
        foreach (Transform child in shopGridContainer) { Destroy(child.gameObject); }
        foreach (Transform child in inventoryGridContainer) { Destroy(child.gameObject); }

        // Создаем новые плашки под каждую пушку
        for (int i = 0; i < allSkins.Length; i++)
        {
            WeaponSkin skin = allSkins[i];

            // Если куплено — в инвентарь, если нет — в магазин
            Transform targetContainer = skin.isPurchased ? inventoryGridContainer : shopGridContainer;

            GameObject newButton = Instantiate(buttonPrefab, targetContainer);

            SkinButton skinButtonScript = newButton.GetComponent<SkinButton>();
            if (skinButtonScript != null)
            {
                skinButtonScript.Setup(skin, this);
            }
        }
    }

    public void OnClickSkin(WeaponSkin skin)
    {
        if (skin.isPurchased)
        {
            foreach (WeaponSkin s in allSkins)
            {
                if (s == skin) s.isEquipped = true;
                else if (s.isEquipped) s.isEquipped = false;
            }
        }
        else
        {
            if (coins >= skin.price)
            {
                coins -= skin.price;
                skin.isPurchased = true;

                foreach (WeaponSkin s in allSkins)
                {
                    if (s == skin) s.isEquipped = true;
                    else if (s.isEquipped) s.isEquipped = false;
                }
            }
            else
            {
                Debug.Log("Не хватает монет!");
                return;
            }
        }

        RefreshWeaponPositions(); 
        UpdateCoinsUI();
        SaveGameData(); // Сохраняем в PlayerPrefs (плагин YG2 сам перехватит и отправит в облако Яндекса)
        SpawnShopButtons(); // Перерисовываем кнопки
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

    // === СОХРАНЕНИЕ (ИДЕАЛЬНО ДЛЯ ПЛАГИНА YG2) ===
    public void SaveGameData()
    {
        PlayerPrefs.SetInt("PlayerCoins", coins);

        for (int i = 0; i < allSkins.Length; i++)
        {
            PlayerPrefs.SetInt("Skin_Purchased_" + i, allSkins[i].isPurchased ? 1 : 0);
            PlayerPrefs.SetInt("Skin_Equipped_" + i, allSkins[i].isEquipped ? 1 : 0);
        }
        PlayerPrefs.Save();
        Debug.Log("Прогресс сохранен в PlayerPrefs и готов к синхронизации с облаком Яндекса!");
    }

    private void LoadGameDataLocal()
    {
        coins = PlayerPrefs.GetInt("PlayerCoins", 1000);
        for (int i = 0; i < allSkins.Length; i++)
        {
            // Первый скин по умолчанию открыт и экипирован, остальные закрыты
            int defaultActive = (i == 0) ? 1 : 0;
            allSkins[i].isPurchased = PlayerPrefs.GetInt("Skin_Purchased_" + i, defaultActive) == 1;
            allSkins[i].isEquipped = PlayerPrefs.GetInt("Skin_Equipped_" + i, defaultActive) == 1;
        }
    }

    [ContextMenu("Reset Progress")]
    public void ResetProgress()
    {
        PlayerPrefs.DeleteAll();
        LoadGameDataLocal();
        UpdateCoinsUI();
        if (Application.isPlaying) SpawnShopButtons();
        Debug.Log("Сохранения полностью стерты и сброшены к дефолту!");
    }
}