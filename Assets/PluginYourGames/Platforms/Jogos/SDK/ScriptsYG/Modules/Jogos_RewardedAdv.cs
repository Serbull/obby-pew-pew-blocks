#if JogosPlatform_yg && RewardedAdv_yg
using JogosGames.Engine.SDK;
using YG.Insides;

namespace YG
{
    public partial class PlatformYG2 : IPlatformsYG2
    {
        public void RewardedAdvShow(string id)
        {
            JogosSDK.Ad.RequestAd(JogosAdType.Rewarded, () =>
            {
                // Callback when ad starts
                YGInsides.OpenRewardedAdv();
            }, (error) =>
            {
                // Callback when ad request fails
                YGInsides.CloseRewardedAdv();
                YGInsides.ErrorRewardedAdv();
            }, () =>
            {
                // Callback when ad finishes
                YGInsides.RewardAdv(id);
                YGInsides.CloseRewardedAdv();
            });
        }
    }
}
#endif