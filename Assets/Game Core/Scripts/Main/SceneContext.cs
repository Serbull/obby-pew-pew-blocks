using UnityEngine;
using Serbull.GameAssets;
using Serbull.GameAssets.Pets;

public class SceneContext : MonoBehaviour
{
    [SerializeField] private GameManager _gameManager;
    [SerializeField] private SGAInstaller _sgaInstaller;
    [SerializeField] private PetInstaller _petInstaller;

    private void Awake()
    {
        var isMobile = YG.YG2.envir.device != YG.YG2.Device.Desktop;
        _sgaInstaller.Init(_gameManager, SaveManager.Data.Roulette, isMobile, YG.YG2.lang);
        _petInstaller.Init(_gameManager, SaveManager.Data.Pets, YG.YG2.lang);

        if (Services.Audio != null)
        {
            Services.Audio.SetSoundVolume(SaveManager.Data.SoundVolume);
            Services.Audio.SetMusicVolume(SaveManager.Data.MusicVolume);
        }
    }
}
