#if BridgePlatform_yg
using Playgama;
using Playgama.Modules.Platform;

namespace YG
{
    public partial class PlatformYG2 : IPlatformsYG2
    {
        public void InitAwake()
        {
            InitAudio();
            InitInterstitialAdv();
            InitRewardedAdv();
            LoadAndCacheCloud(YG2.SyncInitialization);
            Bridge.platform.pauseStateChanged += YG2.PauseGame;
        }

        public void InitStart() { }
        public void InitComplete() { }
        public void GameplayStart() => Bridge.platform.SendMessage(PlatformMessage.GameReady);
        public void GameplayStop() { }
        public void HappyTime() { }
    }
}
#endif