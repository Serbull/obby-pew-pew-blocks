using UnityEngine;
using UnityEngine.UI;
using TMPro; // Добавили, чтобы код понимал TextMeshPro напрямую

public class ShopManager : MonoBehaviour
{
    public int coins = 1000;              // Твой баланс монет
    public WeaponSkin[] allSkins;         // Список всех пушек
    
    public GameObject buttonPrefab;       // Наш шаблон SkinButtonPrefab
    public Transform shopGridContainer;   // Объект Grid из Магазина
    public Transform inventoryGridContainer; // Объект Grid из Инвентаря

    void Start()
    {
        SpawnShopButtons();
    }

    void SpawnShopButtons()
    {
        foreach (Transform child in shopGridContainer) Destroy(child.gameObject);
        foreach (Transform child in inventoryGridContainer) Destroy(child.gameObject);

        foreach (WeaponSkin skin in allSkins)
        {
            // Кнопка в Магазин
            GameObject shopBtn = Instantiate(buttonPrefab, shopGridContainer);
            SetupButton(shopBtn, skin, isShop: true);

            // Кнопка в Инвентарь (показываем только купленное)
            if (skin.isPurchased)
            {
                GameObject invBtn = Instantiate(buttonPrefab, inventoryGridContainer);
                SetupButton(invBtn, skin, isShop: false);
            }
        }
    }

    void SetupButton(GameObject btnObject, WeaponSkin skin, bool isShop)
    {
        // Ищем компоненты автоматически внутри кнопки, без привязки к точным именам!
        Image icon = btnObject.transform.Find("Icon") != null ? 
            btnObject.transform.Find("Icon").GetComponent<Image>() : 
            btnObject.GetComponentInChildren<Image>();

        TextMeshProUGUI btnText = btnObject.GetComponentInChildren<TextMeshProUGUI>();
        Button buttonComp = btnObject.GetComponent<Button>();

        // На всякий случай проверка, чтобы игра точно не вылетала
        if (icon != null && skin.skinIcon != null)
        {
            icon.sprite = skin.skinIcon;
        }

        if (btnText != null)
        {
            if (isShop)
            {
                if (skin.isEquipped) btnText.text = "НАДЕТО";
                else if (skin.isPurchased) btnText.text = "КУПЛЕНО";
                else btnText.text = skin.price + " $";
            }
            else
            {
                if (skin.isEquipped) btnText.text = "НАДЕТО";
                else btnText.text = "НАДЕТЬ";
            }
        }

        if (buttonComp != null)
        {
            buttonComp.onClick.RemoveAllListeners();
            buttonComp.onClick.AddListener(() => OnClickSkin(skin));
        }
    }

    void OnClickSkin(WeaponSkin skin)
    {
        if (!skin.isPurchased)
        {
            if (coins >= skin.price)
            {
                coins -= skin.price;
                skin.isPurchased = true;
            }
            else
            {
                Debug.Log("Мало денег!");
                return;
            }
        }
        else
        {
            foreach (var s in allSkins) s.isEquipped = false;
            skin.isEquipped = true;
        }

        SpawnShopButtons();
    }
}