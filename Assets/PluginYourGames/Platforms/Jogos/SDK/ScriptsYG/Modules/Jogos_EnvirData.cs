#if JogosPlatform_yg && EnvirData_yg
using JogosGames.Engine.SDK;

namespace YG
{
    public partial class PlatformYG2 : IPlatformsYG2
    {
        public void InitEnirData()
        {
            var systemInfo = JogosSDK.SystemInfo;

            YG2.envir.language = systemInfo.language;
            YG2.envir.browser = systemInfo.browser;
            YG2.envir.platform = systemInfo.os;
            YG2.envir.deviceType = systemInfo.deviceType;

            switch (systemInfo.deviceType)
            {
                case "desktop":
                    YG2.envir.isDesktop = true;
                    YG2.envir.isTablet = false;
                    YG2.envir.isMobile = false;
                    YG2.envir.isTV = false;
                    break;
                case "tablet":
                    YG2.envir.isDesktop = false;
                    YG2.envir.isTablet = true;
                    YG2.envir.isMobile = false;
                    YG2.envir.isTV = false;
                    break;
                case "mobile":
                    YG2.envir.isDesktop = false;
                    YG2.envir.isTablet = false;
                    YG2.envir.isMobile = true;
                    YG2.envir.isTV = false;
                    break;
            }
        }

        public void GetEnvirData()
        {
            InitEnirData();
            YG2.GetDataInvoke();
        }
    }
}
#endif
