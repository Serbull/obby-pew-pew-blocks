#if BridgePlatform_yg && EnvirData_yg
using Playgama;
using Playgama.Modules.Device;

namespace YG
{
    public partial class PlatformYG2 : IPlatformsYG2
    {
        public static string browser2;
        public void InitEnirData()
        {
            YG2.envir.language = Bridge.platform.language;

            var device = Bridge.device.type;
            YG2.envir.deviceType = device.ToString().ToLower();

            switch (device)
            {
                case DeviceType.Desktop:
                    YG2.envir.isDesktop = true;
                    YG2.envir.isTablet = false;
                    YG2.envir.isMobile = false;
                    YG2.envir.isTV = false;
                    break;
                case DeviceType.Tablet:
                    YG2.envir.isDesktop = false;
                    YG2.envir.isTablet = true;
                    YG2.envir.isMobile = false;
                    YG2.envir.isTV = false;
                    break;
                case DeviceType.Mobile:
                    YG2.envir.isDesktop = false;
                    YG2.envir.isTablet = false;
                    YG2.envir.isMobile = true;
                    YG2.envir.isTV = false;
                    break;
                case DeviceType.TV:
                    YG2.envir.isDesktop = false;
                    YG2.envir.isTablet = false;
                    YG2.envir.isMobile = false;
                    YG2.envir.isTV = true;
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
