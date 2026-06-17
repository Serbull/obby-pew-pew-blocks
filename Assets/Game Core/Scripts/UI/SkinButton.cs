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
    public Image iconImage;
    public GameObject lockIcon;          // Иконка замка — показывается, если пушка не куплена
    public GameObject selectedHighlight; // Подсветка выбранной кнопки (необязательно)

    private WeaponSkin currentSkin;
    private ShopManager shopManager;

    public WeaponSkin Skin => currentSkin;

    public void Setup(WeaponSkin skin, ShopManager manager)
    {
        currentSkin = skin;
        shopManager = manager;

        // ПЕРЕВОД НАЗВАНИЯ: используем idInHand пушки как ключ локализации,
        // подстраховываемся дефолтным skinName, если перевода нет.
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

        // Замок виден только у некупленной пушки
        if (lockIcon != null) lockIcon.SetActive(!skin.isPurchased);

        // Подсветка — только у реально выбранной (экипированной) пушки
        SetSelected(skin.isEquipped);
    }

    // Подсветка реально выбранной (экипированной) пушки
    public void SetSelected(bool selected)
    {
        if (selectedHighlight != null) selectedHighlight.SetActive(selected);
    }

    // Клик по кнопке только обновляет инфо в боковой панели (без покупки/выбора)
    public void OnClick()
    {
        if (shopManager != null && currentSkin != null)
        {
            shopManager.SelectSkin(currentSkin);
        }
    }
}
