#if BridgePlatform_yg && RewardedAdv_yg
using Playgama;
using Playgama.Modules.Advertisement;
using YG.Insides;

namespace YG
{
    public partial class PlatformYG2 : IPlatformsYG2
    {
        public void InitRewardedAdv()
        {
            Bridge.advertisement.rewardedStateChanged += OnRewardedStateChanged;
        }

        public void RewardedAdvShow(string id)
        {
            Bridge.advertisement.ShowRewarded(id);
        }

        private void OnRewardedStateChanged(RewardedState state)
        {
            switch (state)
            {
                case RewardedState.Loading:
                    break;
                case RewardedState.Opened:
                    YGInsides.OpenRewardedAdv();
                    break;
                case RewardedState.Closed:
                    YGInsides.CloseRewardedAdv();
                    break;
                case RewardedState.Failed:
                    YGInsides.ErrorRewardedAdv();
                    break;
                case RewardedState.Rewarded:
                    YGInsides.RewardAdv(Bridge.advertisement.rewardedPlacement);
                    break;
            }
        }
    }
}
#endif