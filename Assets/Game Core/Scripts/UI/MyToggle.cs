using UnityEngine;
using UnityEngine.UI;

public class MyToggle : MonoBehaviour
{
    [SerializeField] private Image _bg;
    [SerializeField] private GameObject _disabledToggle;

    private void Start()
    {
        var toggle = GetComponent<Toggle>();
        toggle.onValueChanged.AddListener(OnChanged);
        OnChanged(toggle.isOn);
    }

    private void OnChanged(bool isActive)
    {
        _bg.color = isActive ? Color.green : Color.grey;
        _disabledToggle.SetActive(!isActive);
    }
}
