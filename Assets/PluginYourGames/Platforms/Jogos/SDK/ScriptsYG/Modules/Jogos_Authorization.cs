#if JogosPlatform_yg && Authorization_yg
using JogosGames.Engine.SDK;

namespace YG
{
    public partial class PlatformYG2 : IPlatformsYG2
    {
        public void InitAuth()
        {
            // Initialized in script Jogos_BasicAPI
        }

        public void GetAuth()
        {
            JogosSDK.User.GetUser(user =>
            {
                User = user;

                if (user != null)
                {
                    YG2.player.auth = true;
                    YG2.player.name = user.username;
                    YG2.player.photo = user.profilePictureUrl;
                }
                else
                {
                    NotAuthorized();
                }
            });

            YG2.GetDataInvoke();
        }

        public void OpenAuthDialog() {}

        private void NotAuthorized()
        {
            YG2.player.auth = false;
            YG2.player.name = "unauthorized";
            YG2.player.photo = string.Empty;
        }
    }
}
#endif