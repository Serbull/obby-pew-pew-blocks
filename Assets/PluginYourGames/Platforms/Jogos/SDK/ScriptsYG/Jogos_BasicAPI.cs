#if JogosPlatform_yg
using JogosGames.Engine.SDK;

namespace YG
{
    public partial class PlatformYG2 : IPlatformsYG2
    {
        public UserInfo User;

        public void InitAwake()
        {
            JogosSDK.Init(() =>
            {
                SynchronizeToCloud();

                JogosSDK.User.GetUser((user) =>
                {
                    if (user != null)
                    {
                        // Successfully retrieved information    
                        User = user;
#if Authorization_yg
                        YG2.player.auth = true;
                        YG2.player.name = user.username;
                        YG2.player.photo = user.profilePictureUrl;
#endif
                        YG2.SyncInitialization();
                    }
                    else
                    {
                        // Failed to retrieve information
                        YG2.SyncInitialization();
                    }
                });
            });
        }

        public void InitStart() { }
        public void InitComplete() { }
        public void GameplayStart() => JogosSDK.Game.GameplayContinue();
        public void GameplayStop() => JogosSDK.Game.GameplayPause();
        public void HappyTime() => JogosSDK.Game.HappyTime();
    }
}
#endif