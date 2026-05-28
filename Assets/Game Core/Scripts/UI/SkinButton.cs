using UnityEngine;
using UnityEngine.UI;

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

        if (titleText != null) titleText.text = skin.skinName;
        if (iconImage != null) iconImage.sprite = skin.Icon;

        // Настраиваем отображение текста, цветов и галочки
        if (skin.isEquipped)
        {
            if (priceText != null) priceText.text = "АКТИВНО";
            if (statusBackground != null) statusBackground.color = equippedColor;
            
            // Если этот скин выбран — ВКЛЮЧАЕМ галочку
            if (selectedCheckmark != null) selectedCheckmark.SetActive(true);
        }
        else if (skin.isPurchased)
        {
            if (priceText != null) priceText.text = "ВЫБРАТЬ";
            if (statusBackground != null) statusBackground.color = purchasedColor;
            
            // Если куплен, но не выбран — ВЫКЛЮЧАЕМ галочку
            if (selectedCheckmark != null) selectedCheckmark.SetActive(false);
        }
        else
        {
            if (priceText != null) priceText.text = skin.price.ToString() + " $";
            if (statusBackground != null) statusBackground.color = shopColor;
            
            // В магазине галочка тем более не нужна — ВЫКЛЮЧАЕМ
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