using UnityEngine;
using UnityEngine.UI;
using Serbull.GameAssets;
using Serbull.GameAssets.Rare;

// Боковая панель с подробной информацией о выбранной пушке.
// Сама кнопка пушки в списке лишь сообщает сюда, какую пушку показать.
public class WeaponInfoPanel : MonoBehaviour
{
    // Кэшируем конфиг редкостей, чтобы не грузить его из Resources каждый раз
    private static RareConfig _rareConfig;
    private static RareConfig RareConfig =>
        _rareConfig != null ? _rareConfig : (_rareConfig = Resources.Load<RareConfig>("RareConfig"));

    [Header("Refs")]
    public ShopManager shopManager;

    [Header("UI Elements")]
    public TMPro.TextMeshProUGUI nameText;     // Название пушки
    public Image iconImage;                     // Иконка пушки
    public TMPro.TextMeshProUGUI rarityText;   // Редкость пушки (красится в цвет редкости)
    public Slider speedSlider;                  // Скорость пули (0..1 относительно лучшей пушки)
    public Slider forceSlider;                  // Сила толчка (0..1 относительно лучшей пушки)

    [Header("Actions")]
    public Button buyButton;                          // Кнопка КУПИТЬ (если не куплена)
    public TMPro.TextMeshProUGUI buyButtonText;      // Цена на кнопке покупки
    public Button selectButton;                       // Кнопка ВЫБРАТЬ (если куплена, но не экипирована)
    public TMPro.TextMeshProUGUI equippedText;       // Текст «АКТИВНО» (если пушка уже выбрана)

    private WeaponSkin currentSkin;

    public WeaponSkin CurrentSkin => currentSkin;

    void Awake()
    {
        if (shopManager == null) shopManager = FindFirstObjectByType<ShopManager>();

        if (buyButton != null) buyButton.onClick.AddListener(OnBuyClicked);
        if (selectButton != null) selectButton.onClick.AddListener(OnSelectClicked);
    }

    // При открытии попапа показываем инфо о текущей выбранной (экипированной) пушке
    void OnEnable()
    {
        if (shopManager == null) shopManager = FindFirstObjectByType<ShopManager>();
        if (shopManager != null) shopManager.ShowEquippedInfo();
    }

    // Отрисовка инфо о пушке: название, иконка, редкость, характеристики и нужные кнопки
    public void Show(WeaponSkin skin)
    {
        currentSkin = skin;
        if (skin == null) return;

        if (nameText != null)
        {
            string translatedName = Services.Localization.GetText(skin.idInHand);
            nameText.text = !string.IsNullOrEmpty(translatedName) ? translatedName : skin.skinName;
        }

        if (iconImage != null) iconImage.sprite = skin.Icon;

        // Текст редкости + покраска в цвет редкости из RareConfig
        if (rarityText != null && RareConfig != null && !string.IsNullOrEmpty(skin.rareId))
        {
            string translatedRare = Services.Localization.GetText(skin.rareId);
            rarityText.text = !string.IsNullOrEmpty(translatedRare) ? translatedRare : skin.rareId;
            rarityText.color = RareConfig.GetRareData(skin.rareId).Color;
        }

        // Слайдеры характеристик: значение пушки относительно остальных пушек.
        // Нижняя граница слайдера = самый маленький параметр / 2, верхняя = максимум.
        if (speedSlider != null)
            speedSlider.value = Normalize(skin.bulletSpeed, GetMinBulletSpeed() * 0.8f, GetMaxBulletSpeed());
        if (forceSlider != null)
            forceSlider.value = Normalize(skin.forceMultiplier, GetMinForceMultiplier() * 0.8f, GetMaxForceMultiplier());

        bool purchased = skin.isPurchased;
        bool equipped = skin.isEquipped;

        // Кнопка КУПИТЬ — только если пушка ещё не куплена
        if (buyButton != null) buyButton.gameObject.SetActive(!purchased);
        if (buyButtonText != null) buyButtonText.text = skin.price.ToString();

        // Кнопка ВЫБРАТЬ — если куплена, но не экипирована
        if (selectButton != null) selectButton.gameObject.SetActive(purchased && !equipped);

        // Текст «АКТИВНО» — если пушка уже выбрана
        if (equippedText != null) equippedText.gameObject.SetActive(equipped);
    }

    // Нормализуем значение характеристики в диапазон 0..1 от min до max
    private float Normalize(float value, float min, float max)
    {
        return max > min ? Mathf.Clamp01((value - min) / (max - min)) : 0f;
    }

    private float GetMaxBulletSpeed()
    {
        float max = 0f;
        if (shopManager != null && shopManager.allSkins != null)
        {
            foreach (WeaponSkin s in shopManager.allSkins)
            {
                if (s != null && s.bulletSpeed > max) max = s.bulletSpeed;
            }
        }
        return max;
    }

    private float GetMinBulletSpeed()
    {
        float min = float.MaxValue;
        if (shopManager != null && shopManager.allSkins != null)
        {
            foreach (WeaponSkin s in shopManager.allSkins)
            {
                if (s != null && s.bulletSpeed < min) min = s.bulletSpeed;
            }
        }
        return min == float.MaxValue ? 0f : min;
    }

    private float GetMaxForceMultiplier()
    {
        float max = 0f;
        if (shopManager != null && shopManager.allSkins != null)
        {
            foreach (WeaponSkin s in shopManager.allSkins)
            {
                if (s != null && s.forceMultiplier > max) max = s.forceMultiplier;
            }
        }
        return max;
    }

    private float GetMinForceMultiplier()
    {
        float min = float.MaxValue;
        if (shopManager != null && shopManager.allSkins != null)
        {
            foreach (WeaponSkin s in shopManager.allSkins)
            {
                if (s != null && s.forceMultiplier < min) min = s.forceMultiplier;
            }
        }
        return min == float.MaxValue ? 0f : min;
    }

    private void OnBuyClicked()
    {
        if (shopManager != null && currentSkin != null) shopManager.BuySkin(currentSkin);
    }

    private void OnSelectClicked()
    {
        if (shopManager != null && currentSkin != null) shopManager.EquipSkin(currentSkin);
    }
}
