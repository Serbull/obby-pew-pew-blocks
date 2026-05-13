using UnityEngine;

public class PlayerShooter : MonoBehaviour
{
    private Weapon weapon;
    private PlayerWeaponEquip weaponEquip;
    private Animator anim;

    void Start()
    {
        anim = GetComponent<Animator>();
        if (anim == null) anim = GetComponentInChildren<Animator>();
        weaponEquip = GetComponent<PlayerWeaponEquip>();
    }

    void Update()
    {
        if (weaponEquip == null) return;

        if (weapon == null)
        {
            weapon = weaponEquip.GetWeapon();
            return;
        }

        bool isEquipped = (weapon.transform.parent == weaponEquip.handPoint);

        if (isEquipped)
        {
            anim.SetBool("IsAiming", true);

            if ((Input.GetMouseButton(0) || Input.touchCount > 0) && anim.GetFloat("Move") < 1)
            {
                weapon.Shoot();
            }
        }
        else
        {
            anim.SetBool("IsAiming", false);
        }
    }
}