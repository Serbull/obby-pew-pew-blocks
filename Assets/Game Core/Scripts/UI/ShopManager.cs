using UnityEngine;

public class ShopManager : MonoBehaviour
{
    [Header("Shop Settings")]
    public int coins = 1000;              // Твой баланс монет
    public WeaponSkin[] allSkins;         // Список всех пушек

    [Header("UI Containers")]
    public GameObject buttonPrefab;       // Наш шаблон SkinButtonPrefab
    public Transform shopGridContainer;   // Объект Grid из Магазина
    public Transform inventoryGridContainer; // Объект Grid из Инвентаря

    [Header("Weapon Points")]
    public Transform weaponHandPoint;     // Точка в руке игрока
    public Transform weaponBackPoint;     // Точка за спиной игрока

    private GameObject currentHandWeapon; // Текущая пушка в руке
    private GameObject currentBackWeapon; // Текущая пушка за спиной

    void Start()
    {
        SpawnShopButtons();
        RefreshWeaponPositions(); // Расставляем пушки при старте
    }

    public void SpawnShopButtons()
    {
        // Очищаем старые кнопки
        foreach (Transform child in shopGridContainer) Destroy(child.gameObject);
        foreach (Transform child in inventoryGridContainer) Destroy(child.gameObject);

        // Создаем новые кнопки
        foreach (WeaponSkin skin in allSkins)
        {
            // Кнопка в Магазин
            GameObject shopBtn = Instantiate(buttonPrefab, shopGridContainer);
            SetupButton(shopBtn, skin, isShop: true);

            // Кнопка в Инвентарь (только если куплено)
            if (skin.isPurchased)
            {
                GameObject invBtn = Instantiate(buttonPrefab, inventoryGridContainer);
                SetupButton(invBtn, skin, isShop: false);
            }
        }
    }

    void SetupButton(GameObject btnObject, WeaponSkin skin, bool isShop)
    {
        SkinButton sBtn = btnObject.GetComponent<SkinButton>();
        if (sBtn != null)
        {
            sBtn.skinData = skin;
            sBtn.shopManager = this;
            sBtn.isShopButton = isShop;
            sBtn.RenderButton(); // Обновляем внешний вид (текст и картинку)
        }
    }

    public void OnClickSkin(WeaponSkin skin)
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
            // Если пушка куплена — снимаем выбор со всех купленных и надеваем эту
            foreach (var s in allSkins)
            {
                s.isEquipped = false;
            }
            skin.isEquipped = true;
        }

        // Перерисовываем пушки на персонаже и обновляем UI
        RefreshWeaponPositions();
        SpawnShopButtons();
    }

    void RefreshWeaponPositions()
    {
        // 1. Удаляем старые объекты пушек с персонажа
        if (currentHandWeapon != null) Destroy(currentHandWeapon);
        if (currentBackWeapon != null) Destroy(currentBackWeapon);

        // 2. Расставляем пушки по точкам
        foreach (WeaponSkin skin in allSkins)
        {
            if (skin.weaponPrefab == null) continue;

            if (skin.isEquipped)
            {
                // Если пушка надета — спавним в РУКУ
                if (weaponHandPoint != null)
                {
                    currentHandWeapon = Instantiate(skin.weaponPrefab, weaponHandPoint);
                    ResetTransform(currentHandWeapon.transform);
                }
            }
            else if (skin.isPurchased)
            {
                // Если пушка просто куплена (но не надета) — отправляем её ЗА СПИНУ
                // (Примечание: если куплено много пушек, за спиной появится последняя не надетая. 
                // При желании можно сделать логику только для конкретного ствола)
                if (weaponBackPoint != null)
                {
                    currentBackWeapon = Instantiate(skin.weaponPrefab, weaponBackPoint);
                    ResetTransform(currentBackWeapon.transform);
                }
            }
        }
    }

    void ResetTransform(Transform target)
    {
        target.localPosition = Vector3.zero;
        target.localRotation = Quaternion.identity;
    }
}