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
        _sgaInstaller.Init(_gameManager, SaveManager.Data.Roulette, YG.YG2.lang);
        _petInstaller.Init(_gameManager, SaveManager.Data.Pets, YG.YG2.lang);
    }
}
