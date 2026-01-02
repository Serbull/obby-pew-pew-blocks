using UnityEngine;
using TMPro;

[RequireComponent(typeof(TextMeshProUGUI))]
public class LocalizationText : MonoBehaviour
{
    [SerializeField] private string _id;

    private TextMeshProUGUI _text;
    private string _arg0;

    private void Awake()
    {
        _text = GetComponent<TextMeshProUGUI>();
    }

    private void OnEnable()
    {
        LocalizationManager.OnLanguageChanged += UpdateText;
        UpdateText();
    }

    private void OnDisable()
    {
        LocalizationManager.OnLanguageChanged -= UpdateText;
    }

    protected void UpdateText()
    {
        var text = string.Format(LocalizationManager.GetText(_id), _arg0);

        if (_text != null && text != null)
        {
            _text.text = text;
        }
    }

    public void SetLocalizationId(string id, object arg0 = null)
    {
        _id = id;
        _arg0 = arg0?.ToString();
        UpdateText();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (_id != null)
        {
            _id = _id.Replace(" ", "_").ToLower();
        }
    }
#endif
}
