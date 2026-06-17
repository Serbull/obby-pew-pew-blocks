#if JogosPlatform_yg && InterstitialAdv_yg
using JogosGames.Engine.SDK;
using YG.Insides;

namespace YG
{
    public partial class PlatformYG2 : IPlatformsYG2
    {
        public void InterstitialAdvShow()
        {
            JogosSDK.Ad.RequestAd(JogosAdType.Midgame, () =>
            {
                // Callback when ad starts
                YGInsides.OpenInterAdv();
            }, (error) =>
            {
                // Callback when ad request fails
                YGInsides.CloseInterAdv();
                YGInsides.ErrorInterAdv();
            }, () =>
            {
                // Callback when ad finishes
                YGInsides.CloseInterAdv();
            });
        }

        public void FirstInterAdvShow()
        {
            OptionalPlatform.FirstInterAdvShow_RealizationSkip();
        }

        public void OtherInterAdvShow() { }
    }
}
#endif