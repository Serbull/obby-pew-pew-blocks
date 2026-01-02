using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class ChangeMaterialProperty : MonoBehaviour
{
    [SerializeField] private ChangeEvent _changeEventPlayMode;
    [SerializeField] private bool _previewEditorMode = true;
    [Space]
    [SerializeField] private bool _changeTextureTiling;
    [SerializeField] private string _textureTilingName = "_MainTex_ST";
    [SerializeField] private Vector2 _textureScale = Vector2.one;
    [SerializeField] private Vector2 _textureOffset = Vector2.zero;

    [SerializeField] private bool _changeMainColor;
    [SerializeField] private string _mainColorName = "_Color";
    [SerializeField] private Color _mainColor = new Color(1, 1, 1, 1);
    [SerializeField]
    private bool _changeSpecColor;
    [SerializeField]
    private string _specColorName = "_SpecColor";
    [SerializeField]
    private Color _specColor = new Color(0.5f, 0.5f, 0.5f, 1);

    [SerializeField]
    private bool _changeEmissionColor;
    [SerializeField]
    private string _emissionColorName = "_Emission";
    [SerializeField]
    private Color _emissionColor = new Color(0, 0, 0, 0);

    [SerializeField]
    private ChangeFloat[] _changeFloats;

    [SerializeField]
    private ChangeTexture[] _changeTextures;

    [System.Serializable]
    private class ChangeFloat
    {
        [SerializeField]
        private string _floatName;
        [SerializeField]
        private float _floatValue;

        public string FloatName => _floatName;
        public float FloatValue => _floatValue;
    }

    [System.Serializable]
    private class ChangeTexture
    {
        [SerializeField]
        private string _textureName;
        [SerializeField]
        private Texture _texture;

        public string TextureName => _textureName;
        public Texture Texture => _texture;
    }

    private enum ChangeEvent
    {
        Awake,
        Method
    }


    private Renderer _renderer;
    private Renderer Renderer { get { return _renderer != null ? _renderer : GetComponent<Renderer>(); } }
    private MaterialPropertyBlock _property;

    private void OnEnable()
    {
        if ((Application.isPlaying && _changeEventPlayMode == ChangeEvent.Awake) ||
           (!Application.isPlaying && _previewEditorMode))
        {
            UpdateMaterial();
        }
        else
        {
            ResetMaterial();
        }
    }

    public void UpdateMaterial()
    {
        if (_property == null)
        {
            _property = new MaterialPropertyBlock();
        }
        if (Renderer.HasPropertyBlock())
        {
            Renderer.GetPropertyBlock(_property);
        }

        if (_changeTextureTiling)
            _property.SetVector(_textureTilingName, new Vector4(_textureScale.x, _textureScale.y, _textureOffset.x, _textureOffset.y));
        if (_changeMainColor)
            _property.SetColor(_mainColorName, _mainColor);
        if (_changeSpecColor)
            _property.SetColor(_specColorName, _specColor);
        if (_changeEmissionColor)
            _property.SetColor(_emissionColorName, _emissionColor);

        if (_changeFloats != null)
        {
            for (int i = 0; i < _changeFloats.Length; i++)
            {
                _property.SetFloat(_changeFloats[i].FloatName, _changeFloats[i].FloatValue);
            }
        }

        if (_changeTextures != null)
        {
            for (int i = 0; i < _changeTextures.Length; i++)
            {
                if (_changeTextures[i].Texture != null)
                {
                    _property.SetTexture(_changeTextures[i].TextureName, _changeTextures[i].Texture);
                }
            }
        }

        Renderer.SetPropertyBlock(_property);
    }

    public void SetMainColor(Color color)
    {
        _property.SetColor(_mainColorName, color);
        Renderer.SetPropertyBlock(_property);
    }

    public void ResetMaterial()
    {
        Renderer.SetPropertyBlock(null);
    }
}
