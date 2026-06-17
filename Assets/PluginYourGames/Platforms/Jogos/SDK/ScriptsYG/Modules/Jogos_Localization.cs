#if JogosPlatform_yg && Localization_yg
using JogosGames.Engine.SDK;

namespace YG
{
    public partial class PlatformYG2 : IPlatformsYG2
    {
        public string GetLanguage()
        {
            return JogosSDK.SystemInfo.language;
        }
    }
}
#endif