#if CrazyGamesPlatform_yg
using UnityEngine;

namespace YG.Insides
{
    public partial class YGSendMessage : MonoBehaviour
    {
        private void OnApplicationFocus(bool focus)
        {
            if (focus)
            {
#if PaymentsXsolla_yg
                PlatformYG2.TryFetchInventory();
#endif
            }
        }
    }
}
#endif
