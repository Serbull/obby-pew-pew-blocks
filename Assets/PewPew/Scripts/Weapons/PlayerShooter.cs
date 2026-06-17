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

        // 1. Проверяем, над "блокирующим" ли UI палец/курсор (кнопки, магазин).
        // Панель камеры и зона прицеливания считаются сквозными — сквозь них стрелять можно.
        Vector2 pointerPos = Input.touchCount > 0
            ? Input.GetTouch(Input.touchCount - 1).position
            : (Vector2)Input.mousePosition;
        bool isOverUI = UiPassThrough.IsOverBlockingUI(pointerPos);

        if (isEquipped)
        {
            // 2. ИСПРАВЛЕНИЕ: Включаем прицеливание ТОЛЬКО если мышка НЕ над интерфейсом
            if (!isOverUI)
            {
                anim.SetBool("IsAiming", true);

                // Стреляем, только если зажали ЛКМ и НЕ кликаем по UI кнопкам
                if ((Input.GetMouseButton(0) || Input.touchCount > 0) && anim.GetFloat("Move") < 1)
                {
                    weapon.Shoot();
                }
            }
            else
            {
                // Если пушка в руках, но мышка наведена на UI (например, на кнопку "Стоп") — опускаем пушку!
                anim.SetBool("IsAiming", false);
            }
        }
        else
        {
            anim.SetBool("IsAiming", false);
        }
    }
}