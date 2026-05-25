using UnityEngine;

[CreateAssetMenu(fileName = "NewWeaponSkin", menuName = "Market/WeaponSkin")]
public class WeaponSkin : ScriptableObject
{
    public string skinName;
    public int price;
    public Sprite skinIcon;
    public GameObject weaponPrefab; // ВОТ ЭТУ СТРОЧКУ ДОБАВЛЯЕМ! (Сюда закинем 3D-модельку)

    public bool isPurchased;
    public bool isEquipped;
}