using UnityEngine;
using System.Collections;

public class SaveManager : MonoBehaviour
{
    public static YG.SavesYG Data => YG.YG2.saves;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        var obj = new GameObject("Save Manager", typeof(SaveManager));
        DontDestroyOnLoad(obj);
    }

    public static void SaveGameData()
    {
        YG.YG2.SaveProgress();
    }

    private IEnumerator Start()
    {
        while (true)
        {
            yield return new WaitForSeconds(10f);
            SaveGameData();
        }
    }

    private void OnDestroy()
    {
        SaveGameData();
    }

    private void OnApplicationPause(bool pause)
    {
        if (pause)
        {
            SaveGameData();
        }
    }

    private void OnApplicationQuit()
    {
        SaveGameData();
    }
}