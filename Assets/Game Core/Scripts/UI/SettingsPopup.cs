using UnityEngine;
using UnityEngine.UI;

public class SettingsPopup : MonoBehaviour
{
    [SerializeField] private Button _closeButton;
    [SerializeField] private Toggle _musicToggle;
    [SerializeField] private Toggle _soundToggle;
    [SerializeField] private Slider _cameraSlider;

    private void Start()
    {
        _musicToggle.isOn = SaveManager.Data.music == 1;
        _soundToggle.isOn = SaveManager.Data.sound == 1;
        _cameraSlider.value = SaveManager.Data.cameraSensitivity;

        _closeButton.onClick.AddListener(Close);
        _musicToggle.onValueChanged.AddListener(MusicSetActive);
        _soundToggle.onValueChanged.AddListener(SoundSetActive);
        _cameraSlider.onValueChanged.AddListener(CameraSetValue);
    }

    private void Close()
    {
        gameObject.SetActive(false);
    }

    private void MusicSetActive(bool value)
    {
        SaveManager.Data.music = value ? 1 : 0;
    }

    private void SoundSetActive(bool value)
    {
        SaveManager.Data.sound = value ? 1 : 0;
    }

    private void CameraSetValue(float value)
    {
        SaveManager.Data.cameraSensitivity = value;
    }
}
