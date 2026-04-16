#if GameMonetizePlatform_yg

namespace YG
{
    using System.Runtime.InteropServices;

    public partial class PlatformYG2 : IPlatformsYG2
    {
        [DllImport("__Internal")]
        private static extern void InterAdvShow_js();

        public void InterstitialAdvShow()
        {
            InterAdvShow_js();
        }
    }
}

namespace YG.Insides
{
    public partial class YGSendMessage
    {
        public void OpenInterAdv()
        {
            YGInsides.OpenInterAdv();
        }

        public void CloseInterAdv()
        {
            YGInsides.CloseInterAdv();
        }

        public void ErrorInterAdv()
        {
            YGInsides.ErrorInterAdv();
        }
    }
}
#endif