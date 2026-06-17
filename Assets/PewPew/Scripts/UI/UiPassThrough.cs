using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Общая проверка "стреляем/целимся сквозь UI".
/// Панели из whitelist (панель камеры, центральная зона прицеливания и т.д.) НЕ блокируют выстрел,
/// а реальные кнопки/магазин — блокируют. Используется и прицелом, и стрельбой, чтобы логика не разъезжалась.
/// </summary>
public static class UiPassThrough
{
    // UI-элементы, СКВОЗЬ которые можно стрелять/целиться (по вхождению в имя)
    public static readonly List<string> IgnoredUiNames = new List<string>
    {
        "VictoryPanel",
        "DefeatPanel",
        "TopPanel",
        "CenterPanel",
        "LookPad", // панель управления камерой — тапаем по ней, чтобы стрелять
        "Crosshair"
    };

    private static readonly List<RaycastResult> _results = new List<RaycastResult>(16);

    /// <summary>
    /// Палец/курсор сейчас над "блокирующим" UI (кнопка, магазин)?
    /// Панели из whitelist считаются сквозными и возвращают false.
    /// </summary>
    /// <param name="screenPosition">Позиция тача или мыши в экранных координатах.</param>
    public static bool IsOverBlockingUI(Vector2 screenPosition)
    {
        if (EventSystem.current == null) return false;

        PointerEventData eventData = new PointerEventData(EventSystem.current)
        {
            position = screenPosition
        };

        _results.Clear();
        EventSystem.current.RaycastAll(eventData, _results);

        if (_results.Count == 0) return false;

        // Берём верхний UI под пальцем и проверяем его и родителей по whitelist
        Transform current = _results[0].gameObject.transform;
        while (current != null)
        {
            foreach (string ignored in IgnoredUiNames)
            {
                if (current.name.Contains(ignored)) return false; // сквозная панель — не блокируем
            }
            current = current.parent;
        }

        return true; // обычный UI (кнопка, джойстик) — блокируем выстрел
    }
}
