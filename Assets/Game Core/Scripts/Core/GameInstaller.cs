using Serbull.GameAssets;
using UnityEngine;
using YG;

public class GameInstaller : MonoBehaviour
{
    public SGAInstaller sgaInstaller;

    private void Awake()
    {
        sgaInstaller.Init(null, null, YG2.envir.device == YG2.Device.Mobile, YG2.lang);
    }
}
