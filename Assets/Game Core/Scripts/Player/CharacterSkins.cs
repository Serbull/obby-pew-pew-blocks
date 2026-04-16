using System;
using UnityEngine;

[RequireComponent(typeof(CharacterCore))]
public class CharacterSkins : MonoBehaviour
{
    [SerializeField] private GameObject[] _skins;
    [SerializeField] private Sprite[] _icons;

    public int SkinsCount => _skins.Length;
    public Sprite[] Icons => _icons;

    public int CurrentSkinId { get; private set; } = -1;
    public event Action OnSkinChanged;

    private void Start()
    {
        if (GetComponent<CharacterCore>().IsPlayer)
        {
            SetSkinById(SaveManager.Data.SkinId);
        }
        else
        {
            var rndSkinId = UnityEngine.Random.Range(0, _skins.Length);
            SetSkinById(rndSkinId);
        }
    }

    public void SetSkinById(int id)
    {
        CurrentSkinId = id;

        for (int i = 0; i < _skins.Length; i++)
        {
            if (_skins[i] != null)
            {
                _skins[i].SetActive(i == id);
            }
        }

        if (id < 0 || id >= _skins.Length)
        {
            Debug.LogWarning($"Skin id {id} is out of range. Skins count: {_skins.Length}", this);
        }

        OnSkinChanged?.Invoke();
    }
}
