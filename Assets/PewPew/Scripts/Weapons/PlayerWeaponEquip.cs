using UnityEngine;
using System.Collections;

public class PlayerWeaponEquip : MonoBehaviour
{
    public Transform handPoint;
    public Transform backPoint;
    public GameObject weaponPrefab;

    private Weapon weaponInstance;

    // Используем Awake вместо корутины Start, 
    // чтобы пушка спавнилась МОМЕНТАЛЬНО при создании объекта
    void Awake()
    {
        SpawnWeaponOnBack();
    }

    void SpawnWeaponOnBack()
    {
        if (weaponPrefab == null)
        {
            Debug.LogError($"[{gameObject.name}] Не закинут Weapon Prefab в скрипт PlayerWeaponEquip!");
            return;
        }

        GameObject obj = Instantiate(weaponPrefab);
        weaponInstance = obj.GetComponent<Weapon>();

        AttachToBack();
    }

    public void AttachToBack()
    {
        if (weaponInstance == null) return;

        weaponInstance.transform.SetParent(backPoint);
        weaponInstance.transform.localPosition = Vector3.zero;
        weaponInstance.transform.localRotation = Quaternion.identity;
    }

    public void EquipWeapon()
    {
        // Добавим проверку на null, чтобы игра больше никогда не падала из-за этого
        if (weaponInstance == null)
        {
            Debug.LogWarning($"[{gameObject.name}] Ошибка: Попытка взять оружие, которого нет!");
            return;
        }

        weaponInstance.transform.SetParent(handPoint);
        weaponInstance.transform.localPosition = Vector3.zero;
        weaponInstance.transform.localRotation = Quaternion.identity;
    }

    public Weapon GetWeapon()
    {
        return weaponInstance;
    }
}