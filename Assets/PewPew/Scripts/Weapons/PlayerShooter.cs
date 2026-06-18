using System.Collections.Generic;
using UnityEngine;
using YG;

public class PlayerShooter : MonoBehaviour
{
    [Header("Тап против свайпа (мобайл)")]
    [Tooltip("Макс. смещение пальца (в долях высоты экрана), при котором касание ещё считается тапом, а не свайпом камеры.")]
    public float maxTapMoveFraction = 0.03f;

    private Weapon weapon;
    private PlayerWeaponEquip weaponEquip;
    private Animator anim;
    private bool isMobile;

    // Отслеживаем каждое касание, чтобы отличить тап (выстрел) от свайпа (вращение камеры).
    private struct TouchInfo
    {
        public Vector2 startPos;
        public bool isSwipe;
    }

    private readonly Dictionary<int, TouchInfo> trackedTouches = new();

    void Start()
    {
        anim = GetComponent<Animator>();
        if (anim == null) anim = GetComponentInChildren<Animator>();
        weaponEquip = GetComponent<PlayerWeaponEquip>();
        isMobile = !YG2.envir.isDesktop;
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

        // На мобильном поднимаем камеру в режим прицеливания, пока пушка в руках
        // (игрок вошёл в режим игры/дуэли и может стрелять).
        if (isMobile && FollowCameraController.Instance != null)
        {
            FollowCameraController.Instance.SetAimMode(isEquipped);
        }

        // 1. Проверяем, над "блокирующим" ли UI палец/курсор (кнопки, магазин).
        // Панель камеры и зона прицеливания считаются сквозными — сквозь них стрелять можно.
        Vector2 pointerPos = Input.touchCount > 0
            ? Input.GetTouch(Input.touchCount - 1).position
            : (Vector2)Input.mousePosition;
        bool isOverUI = UiPassThrough.IsOverBlockingUI(pointerPos);

        if (isEquipped && !isOverUI)
        {
            anim.SetBool("IsAiming", true);

            // На ПК — автоогонь зажатой ЛКМ. На мобильном — выстрел по тапу (не по свайпу).
            if (isMobile)
            {
                HandleTapShooting(isEquipped);
            }
            else if (Input.GetMouseButton(0) && anim.GetFloat("Move") < 1)
            {
                weapon.Shoot();
            }
        }
        else
        {
            // Если пушка в руках, но палец/мышка над UI (например, на кнопке "Стоп") — опускаем пушку!
            anim.SetBool("IsAiming", false);

            // Всё равно отслеживаем касания, чтобы тап, начавшийся над UI, не выстрелил.
            if (isMobile) HandleTapShooting(isEquipped);
        }
    }

    // Стреляем только если касание оказалось тапом: палец почти не сместился.
    // Свайп (вращение камеры) выстрел не вызывает.
    private void HandleTapShooting(bool isEquipped)
    {
        float moveThreshold = Screen.height * maxTapMoveFraction;

        for (int i = 0; i < Input.touchCount; i++)
        {
            Touch t = Input.GetTouch(i);

            switch (t.phase)
            {
                case TouchPhase.Began:
                    trackedTouches[t.fingerId] = new TouchInfo
                    {
                        startPos = t.position,
                        isSwipe = false
                    };
                    break;

                case TouchPhase.Moved:
                case TouchPhase.Stationary:
                    if (trackedTouches.TryGetValue(t.fingerId, out var moving))
                    {
                        if (!moving.isSwipe && (t.position - moving.startPos).magnitude > moveThreshold)
                        {
                            moving.isSwipe = true;
                            trackedTouches[t.fingerId] = moving;
                        }
                    }
                    break;

                case TouchPhase.Ended:
                    if (trackedTouches.TryGetValue(t.fingerId, out var ended))
                    {
                        bool movedTooFar = (t.position - ended.startPos).magnitude > moveThreshold;
                        bool isTap = !ended.isSwipe && !movedTooFar;

                        if (isTap && isEquipped
                            && anim.GetFloat("Move") < 1
                            && !UiPassThrough.IsOverBlockingUI(t.position))
                        {
                            weapon.Shoot();
                        }

                        trackedTouches.Remove(t.fingerId);
                    }
                    break;

                case TouchPhase.Canceled:
                    trackedTouches.Remove(t.fingerId);
                    break;
            }
        }
    }
}
