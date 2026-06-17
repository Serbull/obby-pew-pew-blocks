using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using YG;

public class CursorController : MonoBehaviour
{
    public RectTransform crosshair;
    public Animator playerAnimator;

    // Список названий UI-элементов, сквозь которые МОЖНО стрелять/целиться.
    // Единый источник — UiPassThrough, чтобы прицел и стрельба не расходились.
    private readonly List<string> ignoredUiNames = UiPassThrough.IgnoredUiNames;

    void Update()
    {
        if (playerAnimator == null || crosshair == null) return;

        bool isMobile = !YG2.envir.isDesktop;
        bool isOverUI = false;

        // 1. Проверяем UI в зависимости от платформы
        if (isMobile)
        {
            if (EventSystem.current != null)
            {
                for (int i = 0; i < Input.touchCount; i++)
                {
                    Touch touch = Input.GetTouch(i);
                    // Проверяем, попал ли тач на UI
                    if (EventSystem.current.IsPointerOverGameObject(touch.fingerId))
                    {
                        // Если попал, проверяем — это системный UI (кнопки) или игнорируемые панели
                        if (!CheckIfHitIgnoredUI(touch.fingerId))
                        {
                            isOverUI = true;
                            break;
                        }
                    }
                }
            }
        }
        else
        {
            // Проверка для ПК (мышь)
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            {
                // Если мышка над UI, проверяем, не игнорируемый ли это элемент
                if (!CheckIfHitIgnoredUI(-1)) // -1 для мышки в EventSystem
                {
                    isOverUI = true;
                }
            }
        }

        // 2. Считываем состояние прицеливания из аниматора
        bool isAiming = playerAnimator.GetBool("IsAiming");

        // 3. Управляем активностью крестика прицела.
        // На мобильном целимся точкой касания, поэтому крестик не показываем вовсе.
        crosshair.gameObject.SetActive(!isMobile && isAiming && !isOverUI);

        // 4. Позиционируем прицел (только на ПК — на мобильном крестик скрыт)
        if (!isMobile)
        {
            if (isAiming && !isOverUI)
            {
                crosshair.position = Input.mousePosition;
            }
            else
            {
                crosshair.position = new Vector3(Screen.width / 2f, Screen.height / 2f, 0);
            }
        }

        // 5. Настраиваем системный курсор
        ApplyCursor(isAiming, isOverUI, isMobile);
    }

    void ApplyCursor(bool isAiming, bool isOverUI, bool isMobile)
    {
        if (isMobile)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            Cursor.visible = !isAiming || isOverUI;
            Cursor.lockState = CursorLockMode.None;
        }
    }

    // Вспомогательный метод: проверяет, имя какого UI-объекта мы сейчас задели
    private bool CheckIfHitIgnoredUI(int pointerId)
    {
        if (EventSystem.current == null) return false;

        // Создаем контейнер для данных клика/тача
        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);

        // Передаем координаты в зависимости от того, мышка это или палец
        if (pointerId == -1)
            eventDataCurrentPosition.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        else if (Input.touchCount > 0)
            eventDataCurrentPosition.position = Input.GetTouch(Mathf.Clamp(pointerId, 0, Input.touchCount - 1)).position;

        // Пускаем луч (Raycast) по UI элементам в точке клика
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);

        if (results.Count > 0)
        {
            // Берем самый первый UI объект, в который попал луч
            GameObject hitObj = results[0].gameObject;

            // Проверяем его имя и имена его родителей вверх по иерархии
            Transform currentCheck = hitObj.transform;
            while (currentCheck != null)
            {
                foreach (string ignoredName in ignoredUiNames)
                {
                    // Если имя содержит искомое слово (регистр важен, пиши как в иерархии!)
                    if (currentCheck.name.Contains(ignoredName))
                    {
                        return true; // Нашли совпадение, этот UI нужно проигнорировать!
                    }
                }
                currentCheck = currentCheck.parent;
            }
        }

        return false; // Попали на обычный интерфейс (джойстик, кнопки и т.д.)
    }
}