#if BridgePlatform_yg && Authorization_yg
using Playgama;

namespace YG
{
    public partial class PlatformYG2 : IPlatformsYG2
    {
        public void InitAuth()
        {
            UpdateInfo();
        }

        public void GetAuth()
        {
            UpdateInfo();
        }

        public void OpenAuthDialog()
        {
            if (!Bridge.player.isAuthorizationSupported)
                return;

            Bridge.player.Authorize(null, (success) =>
            {
                UpdateInfo();
            });
        }

        private void UpdateInfo()
        {
            YG2.player.auth = Bridge.player.isAuthorized;
            YG2.player.name = Bridge.player.name;
            YG2.player.photo = Bridge.player.photos.Count > 0 ? Bridge.player.photos[0] : null;

            YG2.Message("YG player:");
            YG2.Message(UnityEngine.JsonUtility.ToJson(YG2.player));

            YG2.GetDataInvoke();
        }
    }
}
#endif