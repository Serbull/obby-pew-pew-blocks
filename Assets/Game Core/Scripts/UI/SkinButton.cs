using UnityEngine;
using UnityEngine.UI;
using Serbull.GameAssets;
using Serbull.GameAssets.Rare;

public class SkinButton : MonoBehaviour
{
    // Кэшируем конфиг редкостей, чтобы не грузить его из Resources для каждой кнопки
    private static RareConfig _rareConfig;
    private static RareConfig RareConfig =>
        _rareConfig != null ? _rareConfig : (_rareConfig = Resources.Load<RareConfig>("RareConfig"));

    [Header("UI Elements")]
    public Image background;
    public TMPro.TextMeshProUGUI titleText;
    public TMPro.TextMeshProUGUI priceText;
    public TMPro.TextMeshProUGUI speedText;  // Скорость пули
    public TMPro.TextMeshProUGUI forceText;  // Множитель силы толчка
    public Image iconImage;
    public Image statusBackground;
    public GameObject selectedCheckmark; // Переменная для нашей галочки!

    [Header("Colors")]
    public Color equippedColor = Color.green;
    public Color purchasedColor = Color.blue;
    public Color shopColor = Color.gray;

    private WeaponSkin currentSkin;
    private ShopManager shopManager;

    public void Setup(WeaponSkin skin, ShopManager manager)
    {
        currentSkin = skin;
        shopManager = manager;

        // ПЕРЕВОД НАЗВАНИЯ: Используем idInHand пушки как ключ локализации.
        // Если перевода нет (например, забыл добавить в конфиг), можно подстраховаться и оставить дефолтный skinName.
        if (titleText != null && skin != null)
        {
            string translatedName = Services.Localization.GetText(skin.idInHand);
            titleText.text = !string.IsNullOrEmpty(translatedName) ? translatedName : skin.skinName;
        }

        if (iconImage != null) iconImage.sprite = skin.Icon;

        // Красим фон кнопки в цвет редкости пушки из RareConfig
        if (background != null && RareConfig != null && !string.IsNullOrEmpty(skin.rareId))
        {
            background.color = RareConfig.GetRareData(skin.rareId).Color;
        }

        // Отображаем характеристики пушки: скорость пули и множитель силы толчка
        if (speedText != null) speedText.text = skin.bulletSpeed.ToString("0.#");
        if (forceText != null) forceText.text = skin.forceMultiplier.ToString("0.#");

        // Настраиваем отображение текста, цветов и галочки с учетом локализации
        if (skin.isEquipped)
        {
            // ПЕРЕВОД: "АКТИВНО"
            if (priceText != null) priceText.text = Services.Localization.GetText("ui_skin_active");
            if (statusBackground != null) statusBackground.color = equippedColor;

            if (selectedCheckmark != null) selectedCheckmark.SetActive(true);
        }
        else if (skin.isPurchased)
        {
            // ПЕРЕВОД: "ВЫБРАТЬ"
            if (priceText != null) priceText.text = Services.Localization.GetText("ui_skin_select");
            if (statusBackground != null) statusBackground.color = purchasedColor;

            if (selectedCheckmark != null) selectedCheckmark.SetActive(false);
        }
        else
        {
            // Здесь просто цена, локализация значка валюты по желанию (оставил как у тебя)
            if (priceText != null) priceText.text = skin.price.ToString() + " $";
            if (statusBackground != null) statusBackground.color = shopColor;

            if (selectedCheckmark != null) selectedCheckmark.SetActive(false);
        }
    }

    public void OnClick()
    {
        if (shopManager != null && currentSkin != null)
        {
            shopManager.OnClickSkin(currentSkin);
        }
    }
}