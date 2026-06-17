#if BridgePlatform_yg && Localization_yg
using Playgama;

namespace YG
{
    public partial class PlatformYG2 : IPlatformsYG2
    {
        public string GetLanguage()
        {
            return Bridge.platform.language;
        }
    }
}
#endif