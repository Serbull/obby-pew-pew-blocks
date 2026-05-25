using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SkinButton : MonoBehaviour
{
    [Header("Data Links")]
    public WeaponSkin skinData;     // Данные о пушке
    public ShopManager shopManager; // Ссылка на менеджер магазина

    [Header("UI Elements")]
    public TextMeshProUGUI nameText;  // Название пушки (снизу ячейки)
    public Image iconImage;           // Иконка пушки
    public Button actionButton;       // Сама кнопка
    public GameObject equippedCheck;  // Объект зеленой галочки в углу ячейки

    [HideInInspector]
    public bool isShopButton;         // Магазин или инвентарь?

    void Start()
    {
        if (actionButton != null)
        {
            actionButton.onClick.AddListener(OnClick);
        }
    }

    public void RenderButton()
    {
        if (skinData == null) return;

        // Ставим иконку
        if (iconImage != null) iconImage.sprite = skinData.skinIcon;

        if (isShopButton)
        {
            // В магазине пишем: "Название" и ниже цену (если не куплено)
            if (nameText != null)
            {
                string priceStatus = skinData.isPurchased ? "КУПЛЕНО" : skinData.price.ToString() + " $";
                nameText.text = $"{skinData.skinName}\n<size=75%>{priceStatus}</size>";
            }
            if (equippedCheck != null) equippedCheck.SetActive(false); // В магазине галочка не нужна
        }
        else
        {
            // В инвентаре пишем просто Название пушки
            if (nameText != null)
            {
                nameText.text = skinData.skinName;
            }

            // Включаем зеленую галочку ТОЛЬКО если пушка сейчас надета!
            if (equippedCheck != null)
            {
                equippedCheck.SetActive(skinData.isEquipped);
            }
        }
    }

    void OnClick()
    {
        if (shopManager != null && skinData != null)
        {
            shopManager.OnClickSkin(skinData);
        }
    }
}