using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Serbull.GameAssets;

[RequireComponent(typeof(Button))]
public class AFKToggle : MonoBehaviour
{
    [Header("Ссылки")]
    public GameController gameController;
    public TextMeshProUGUI label;          // Текст на кнопке

    [Header("Цвета")]
    public Color onColor = new Color(0.2f, 0.8f, 0.3f);    // Зелёный — AFK вкл
    public Color offColor = new Color(0.8f, 0.25f, 0.25f); // Красный — AFK выкл

    [Header("Старт")]
    public bool startOn = false;

    private Button _button;
    private bool _isAFK;

    private void Start()
    {
        _button = GetComponent<Button>();
        _button.onClick.AddListener(Toggle);

        if (gameController == null)
            gameController = FindFirstObjectByType<GameController>();

        SetAFK(startOn);
    }

    private void Toggle()
    {
        SetAFK(!_isAFK);
    }

    private void SetAFK(bool isOn)
    {
        _isAFK = isOn;

        // Переключаем AFK в контроллере
        if (gameController != null) gameController.ToggleAFK(isOn);

        // Меняем локализованный текст
        if (label != null)
        {
            label.text = Services.Localization.GetText(isOn ? "afk_on" : "afk_off");
        }

        // Меняем цвет
        if (_button.targetGraphic != null)
        {
            _button.targetGraphic.color = isOn ? onColor : offColor;
        }
    }
}
