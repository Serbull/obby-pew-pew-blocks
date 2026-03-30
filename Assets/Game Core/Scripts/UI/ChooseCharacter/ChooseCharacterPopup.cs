using UnityEngine;
using UnityEngine.UI;

public class ChooseCharacterPopup : MonoBehaviour
{
    [SerializeField] private ChooseCharacterItem[] _characterItems;
    [SerializeField] private Button _closeButton;

    private void Awake()
    {
        if (_closeButton != null)
        {
            _closeButton.onClick.AddListener(OnCloseButtonClicked);
        }

        foreach (var item in _characterItems)
        {
            item.Init(OnCharacterSelected);
        }
    }

    private void OnEnable()
    {
        var skinId = SaveManager.Data.SkinId;
        foreach (var item in _characterItems)
        {
            item.SetSelected(skinId == item.Id);
        }
    }

    private void OnCharacterSelected(ChooseCharacterItem selectedItem)
    {
        SaveManager.Data.IsFirstSkinSelected = true;
        SaveManager.Data.SkinId = selectedItem.Id;
        PlayerController.Instance.CharacterCore.GetComponent<CharacterSkins>().SetSkinById(selectedItem.Id);

        foreach (var item in _characterItems)
        {
            item.SetSelected(selectedItem.Id == item.Id);
        }
    }

    private void OnCloseButtonClicked()
    {
        gameObject.SetActive(false);
    }
}
