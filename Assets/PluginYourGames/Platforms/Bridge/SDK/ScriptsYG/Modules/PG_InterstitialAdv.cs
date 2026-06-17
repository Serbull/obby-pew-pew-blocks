#if BridgePlatform_yg && InterstitialAdv_yg
using Playgama;
using Playgama.Modules.Advertisement;
using YG.Insides;

namespace YG
{
    public partial class PlatformYG2 : IPlatformsYG2
    {
        public void InitInterstitialAdv()
        {
            //Bridge.advertisement.SetMinimumDelayBetweenInterstitial(YG2.interAdvInterval);

            Bridge.advertisement.interstitialStateChanged += OnInterstitialStateChanged;
        }

        public void InterstitialAdvShow()
        {
            Bridge.advertisement.ShowInterstitial();
        }

        public void FirstInterAdvShow()
        {
            OptionalPlatform.FirstInterAdvShow_RealizationSkip();
        }

        public void OtherInterAdvShow() { }

        private void OnInterstitialStateChanged(InterstitialState state)
        {
            switch (state)
            {
                case InterstitialState.Loading:
                    break;
                case InterstitialState.Opened:
                    YGInsides.OpenInterAdv();
                    break;
                case InterstitialState.Closed:
                    YGInsides.CloseInterAdv();
                    break;
                case InterstitialState.Failed:
                    YGInsides.ErrorInterAdv();
                    break;
            }
        }
    }
}
#endif