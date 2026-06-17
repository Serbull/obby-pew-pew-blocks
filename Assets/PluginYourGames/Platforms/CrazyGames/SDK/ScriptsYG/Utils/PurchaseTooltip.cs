#if CrazyGamesPlatform_yg
using UnityEngine;
using UnityEngine.UI;

public class PurchaseTooltip : MonoBehaviour
{
    private static PurchaseTooltip _instance;

    [SerializeField] private float _showDuration = 3f;
    [SerializeField] private float _animationDuration = 0.2f;
    [SerializeField] private Button _closeButton;
    [SerializeField] private Transform _panel;
    [SerializeField] private Transform _loaderImage;

    private float _currentShowingTime;

    public static void Show()
    {
        if (_instance == null)
        {
            var prefab = Resources.Load<PurchaseTooltip>("XsollaPurchaseTooltip");
            if (prefab)
            {
                _instance = Instantiate(prefab);
            }
        }

        if (_instance != null)
        {
            _instance.gameObject.SetActive(true);
        }
    }

    private void Awake()
    {
        if (_closeButton)
        {
            _closeButton.onClick.AddListener(() => gameObject.SetActive(false));
        }
    }

    private void OnEnable()
    {
        _currentShowingTime = 0;
    }

    private void Update()
    {
        _currentShowingTime += Time.unscaledDeltaTime;

        if (_currentShowingTime < _animationDuration && _panel)
        {
            var scale = Mathf.Clamp01(_currentShowingTime / _animationDuration);
            _panel.localScale = Vector3.one * scale;
        }

        if (_currentShowingTime >= _showDuration)
        {
            gameObject.SetActive(false);
            return;
        }

        if (_loaderImage)
        {
            _loaderImage.Rotate(Vector3.forward, -180f * Time.unscaledDeltaTime);
        }
    }
}
#endif
