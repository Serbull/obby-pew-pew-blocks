using UnityEngine;
#if BridgePlatform_yg
using Playgama;
#endif

namespace YG
{
    public partial class YG2
    {
        public static bool IsPaymentsSupported()
        {
#if BridgePlatform_yg
            return Bridge.payments.isSupported;
#else
            return true;
#endif
        }
    }
}
