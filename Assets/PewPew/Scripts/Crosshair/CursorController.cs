using UnityEngine;
using UnityEngine.EventSystems; // Оставляем для мышки

public class CursorController : MonoBehaviour
{
    public RectTransform crosshair;
    public Animator playerAnimator;

    void Update()
    {
        if (playerAnimator == null || crosshair == null) return;

        // 1. Проверяем UI только для того, чтобы вовремя вернуть стрелочку мыши
        bool isOverUI = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();

        // 2. Просто считываем IsAiming (PlayerShooter сам им теперь управляет)
        bool isAiming = playerAnimator.GetBool("IsAiming");

        // 3. Крестик прицела активен, только если мы целимся и НЕ водим по кнопкам
        crosshair.gameObject.SetActive(isAiming && !isOverUI);

        if (isAiming && !isOverUI)
        {
            crosshair.position = Input.mousePosition;
        }
        else
        {
            crosshair.position = new Vector3(Screen.width / 2f, Screen.height / 2f, 0);
        }

        // 4. Включаем/выключаем курсор
        ApplyCursor(isAiming, isOverUI);
    }

    void ApplyCursor(bool isAiming, bool isOverUI)
    {
        // Стрелочка мыши появляется, если мы НЕ целимся ИЛИ если мышка заехала на UI
        Cursor.visible = !isAiming || isOverUI;

        // Оставляем None, чтобы мышь свободно летала по магазину
        Cursor.lockState = CursorLockMode.None;
    }
}