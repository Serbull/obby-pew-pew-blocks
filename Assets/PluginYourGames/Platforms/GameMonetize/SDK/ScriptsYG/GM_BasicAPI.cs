#if GameMonetizePlatform_yg
namespace YG
{
    using UnityEngine;
#if UNITY_WEBGL
    using System.Runtime.InteropServices;
#endif

    public partial class PlatformYG2 : IPlatformsYG2
    {
#if UNITY_WEBGL
        [DllImport("__Internal")]
        private static extern void InitSDK_js(string gameId);
#endif
        public void InitAwake()
        {
            if (YG2.infoYG.platformInfo.gameMonetizeID == string.Empty)
            {
                Debug.LogError("GameMonetize game ID is EMPTY!");
            }

#if !UNITY_EDITOR
            InitSDK_js(YG2.infoYG.platformInfo.gameMonetizeID);
#endif
            YG2.SyncInitialization();
        }
    }
}
#endif