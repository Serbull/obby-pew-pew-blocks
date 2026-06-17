using UnityEngine;
using YG;

namespace SMP
{
    public class PurchaseService : MonoBehaviour
    {
        private static bool _isConsumed;

        private void Start()
        {
            YG2.onPurchaseSuccess += SuccessPurchased;

            if (!_isConsumed)
            {
                _isConsumed = true;
                YG2.ConsumePurchases();
            }
        }

        private void OnDestroy()
        {
            YG2.onPurchaseSuccess -= SuccessPurchased;
        }

        private void SuccessPurchased(string id)
        {
            throw new System.NotImplementedException();
        }
    }
}
