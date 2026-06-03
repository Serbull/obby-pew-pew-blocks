using UnityEngine;
using System.Collections;

public class PlayerWeaponEquip : MonoBehaviour
{
    public Transform handPoint;
    public Transform backPoint;
    public GameObject weaponPrefab;

    private Weapon weaponInstance;

    IEnumerator Start()
    {
        yield return null;
        yield return new WaitForEndOfFrame();

        SpawnWeaponOnBack();
    }

    void SpawnWeaponOnBack()
    {
        GameObject obj = Instantiate(weaponPrefab);
        weaponInstance = obj.GetComponent<Weapon>();

        AttachToBack();
    }

    public void AttachToBack()
    {
        weaponInstance.transform.SetParent(backPoint);
        weaponInstance.transform.localPosition = Vector3.zero;
        weaponInstance.transform.localRotation = Quaternion.identity;
    }

    public void EquipWeapon()
    {
        weaponInstance.transform.SetParent(handPoint);
        weaponInstance.transform.localPosition = Vector3.zero;
        weaponInstance.transform.localRotation = Quaternion.identity;
    }

    public Weapon GetWeapon()
    {
        return weaponInstance;
    }

    // временно для теста
    // void Update()
    // {
    //     if (Input.GetKeyDown(KeyCode.F))
    //         EquipWeapon();

    // 	if (Input.GetKeyDown(KeyCode.G)) // добавим возврат на спину для теста
    //         AttachToBack();
    // }
}