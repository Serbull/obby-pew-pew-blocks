using UnityEngine;
using UnityEngine.UI;
using Serbull.GameAssets;

public class SkinButton : MonoBehaviour
{
    [Header("UI Elements")]
    public TMPro.TextMeshProUGUI titleText;
    public TMPro.TextMeshProUGUI priceText;
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