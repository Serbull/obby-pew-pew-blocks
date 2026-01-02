using UnityEngine;
using UnityEngine.UI;
using Serbull.GameAssets;

public class SettingsPopup : MonoBehaviour
{
    [SerializeField] private Button _closeButton;
    [SerializeField] private Toggle _musicToggle;
    [SerializeField] private Toggle _soundToggle;
    [SerializeField] private Slider _cameraSlider;

    private void Start()
    {
        _musicToggle.isOn = SaveManager.Data.MusicVolume > 0;
        _soundToggle.isOn = SaveManager.Data.SoundVolume > 0;
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
        SaveManager.Data.MusicVolume = value ? 1 : 0;
        Services.Audio?.SetMusicVolume(SaveManager.Data.MusicVolume);
    }

    private void SoundSetActive(bool value)
    {
        SaveManager.Data.SoundVolume = value ? 1 : 0;
        Services.Audio?.SetSoundVolume(SaveManager.Data.SoundVolume);
    }

    private void CameraSetValue(float value)
    {
        SaveManager.Data.cameraSensitivity = value;
    }
}
