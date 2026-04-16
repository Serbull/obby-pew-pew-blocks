using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    //private void Start()
    //{
    //    YaGamesSDK.CloudSaves.OnDataLoaded += CheckLoading;
    //    YaGamesSDK.Flags.OnLoaded += CheckLoading;
    //    CheckLoading();
    //}

    //private void OnDestroy()
    //{
    //    YaGamesSDK.CloudSaves.OnDataLoaded -= CheckLoading;
    //    YaGamesSDK.Flags.OnLoaded -= CheckLoading;
    //}

    //private void CheckLoading()
    //{
    //    if (!YaGamesSDK.Flags.IsLoaded) return;

    //    if (!YaGamesSDK.CloudSaves.IsDataLoaded) return;

    //    SaveManager.LoadGameData();
    //    SceneManager.LoadScene(1);
    //}
}
