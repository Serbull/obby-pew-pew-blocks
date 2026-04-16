using System;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ChooseCharacterItem : MonoBehaviour
{
    [SerializeField] private Image _backgroundImage;
    [SerializeField] private Image _characterImage;
    [SerializeField] private GameObject _checkmark;
    [SerializeField] private bool _scaled = true;
    [SerializeField] private bool _darkened = true;
    public int Id;

    public void Init(Action<ChooseCharacterItem> callback)
    {
        var button = GetComponent<Button>();
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => callback(this));
    }

    public void SetSelected(bool selected)
    {
        if (_backgroundImage != null)
        {
            _backgroundImage.color = selected ? Color.green : Color.black;
        }

        if (_darkened)
        {
            _characterImage.color = selected ? Color.white : Color.gray;
        }

        if (_scaled)
        {
            transform.localScale = selected ? Vector3.one * 1.2f : Vector3.one;
        }

        if (_checkmark != null)
        {
            _checkmark.SetActive(selected);
        }
    }
}
