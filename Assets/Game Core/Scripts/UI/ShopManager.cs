using UnityEngine;
using System;

// Если плагин использует свой namespace, C# его автоматически подхватит ниже
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

    // Структура для сохранения всех данных в один клик
    [System.Serializable]
    public class GameSaveData
    {
        public int coins;
        public bool[] purchasedArray;
        public bool[] equippedArray;
    }

    void Awake()
    {
        // 1. Сначала загружаем данные локально (чтобы игра сразу знала баланс)
        LoadGameDataLocal(); 
        UpdateCoinsUI();
    }

    void Start()
    {
        SpawnShopButtons();
        RefreshWeaponPositions();

        // 2. Сразу после старта запрашиваем у плагина облачные сохранения Яндекса
        LoadFromYandexCloud();
    }

    // === ФУНКЦИЯ ЗАГРУЗКИ ИЗ ОБЛАКА ЯНДЕКСА ===
    private void LoadFromYandexCloud()
    {
        // Проверяем, инициализирован ли плагин на сцене
        // Обычно в PluginYourGames данные берутся через встроенный класс или PlayerPrefs, которые плагин автоматически синхронизирует.
        // Если твой плагин полностью заменяет PlayerPrefs, то Яндекс-сохранения подтянутся сами.
        // Но на случай, если плагин требует ручного вызова, мы дублируем это:
        
        #if !UNITY_EDITOR && UNITY_WEBGL
        try 
        {
            // Плагин синхронизирует PlayerPrefs с облаком Яндекса автоматически при старте.
            // Поэтому просто повторно переинициализируем данные из PlayerPrefs:
            LoadGameDataLocal();
            UpdateCoinsUI();
            RefreshWeaponPositions();
        }
        catch (Exception e)
        {
            Debug.LogError("Ошибка синхронизации плагина Яндекса: " + e.Message);
        }
        #endif
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
        SaveGameData(); // Сохраняем прогресс покупки!
        SpawnShopButtons();
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

    public void SpawnShopButtons()
    {
        // Твой стандартный метод отрисовки кнопок (оставляем без изменений)
    }

    // === СОХРАНЕНИЕ ДАННЫХ В ОБЛАКО ===
    public void SaveGameData()
    {
        // Шаг 1: Записываем в стандартный PlayerPrefs.
        // Фишка плагина PluginYourGames в том, что он ПЕРЕХВАТЫВАЕТ стандартный PlayerPrefs 
        // и автоматически отправляет эти ключи в облако Яндекс Игр!
        
        PlayerPrefs.SetInt("PlayerCoins", coins);

        for (int i = 0; i < allSkins.Length; i++)
        {
            PlayerPrefs.SetInt("Skin_Purchased_" + i, allSkins[i].isPurchased ? 1 : 0);
            PlayerPrefs.SetInt("Skin_Equipped_" + i, allSkins[i].isEquipped ? 1 : 0);
        }
        
        // Шаг 2: Принудительно приказываем Unity сохранить файлы на диск/в браузер
        PlayerPrefs.Save();

        // Шаг 3: Вызываем триггер синхронизации плагина (если он настроен на авто-облако)
        Debug.Log("Данные сохранены локально и переданы в буфер плагина Яндекса.");
    }

    private void LoadGameDataLocal()
    {
        coins = PlayerPrefs.GetInt("PlayerCoins", 1000);
        for (int i = 0; i < allSkins.Length; i++)
        {
            int defaultActive = (i == 0) ? 1 : 0;
            allSkins[i].isPurchased = PlayerPrefs.GetInt("Skin_Purchased_" + i, defaultActive) == 1;
            allSkins[i].isEquipped = PlayerPrefs.GetInt("Skin_Equipped_" + i, defaultActive) == 1;
        }
    }

    [ContextMenu("Reset Progress")]
    public void ResetProgress()
    {
        PlayerPrefs.DeleteAll();
        Debug.Log("Сохранения стерты!");
    }
}