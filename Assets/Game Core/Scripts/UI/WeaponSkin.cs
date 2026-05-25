using UnityEngine;

[CreateAssetMenu(fileName = "NewSkin", menuName = "Shop/Weapon Skin")]
public class WeaponSkin : ScriptableObject
{
    public string skinName;
    public int price;
    public Sprite skinIcon;
    public bool isPurchased;
    public bool isEquipped;
}