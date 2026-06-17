using UnityEngine;

[CreateAssetMenu(fileName = "NewSkin", menuName = "Weapon Skin")]
public class WeaponSkin : ScriptableObject
{
    [Header("UI Display")]
    public string skinName;             // Название пушки для магазина

    [Header("Identity Link (CRITICAL)")]
    [Tooltip("Впиши сюда ТОЧНОЕ имя объекта, как он называется внутри WaponPrefab в руке персонажа!")]
    public string idInHand;             // Строка для поиска (flaregun, DesertEagle, M1911 Handgun)

    [Header("Weapon Stats")]
    [Tooltip("Скорость движения пули")]
    public float bulletSpeed = 10f;     // Скорость полёта пули

    [Tooltip("Мультипликатор силы, с которой пуля толкает блок")]
    public float forceMultiplier = 1f;  // Множитель силы толчка блока

    [Header("Rarity")]
    [Tooltip("Id редкости из RareConfig: common, uncommon, rare, epic, legendary, mystical, secret")]
    [Serbull.GameAssets.Rare.RareDropdown] public string rareId = "common";    // Редкость пушки (ключ из RareConfig)

    [Header("Shop Settings")]
    public int price;                   // Цена пушки

    // Сделали "Icon" с большой буквы, чтобы SkinButton на строчке 33 был счастлив!
    public Sprite Icon;

    public GameObject weaponPrefab;     // Ссылка на префаб

    [Header("Save States")]
    public bool isPurchased;
    public bool isEquipped;
}