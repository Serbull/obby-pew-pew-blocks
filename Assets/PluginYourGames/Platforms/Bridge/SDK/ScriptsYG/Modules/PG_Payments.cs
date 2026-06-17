#if BridgePlatform_yg
using UnityEngine;
using YG.Insides;
using Playgama;
using System.Collections.Generic;

namespace YG
{
    public partial class PlatformYG2 : IPlatformsYG2
    {
        public void InitPayments()
        {
            if (Bridge.payments.isSupported)
            {
                Bridge.payments.GetCatalog(OnGetCatalogCompleted);
            }
        }

        public void BuyPayments(string id)
        {
            if (Bridge.payments.isSupported)
            {
                Bridge.payments.Purchase(id, OnPurchaseCompleted);
            }
            else
            {
                YG2.PauseGame(false);
            }
        }

        public void ConsumePurchaseByID(string id, bool onPurchaseSuccess)
        {
            if (Bridge.payments.isSupported)
            {
                Bridge.payments.ConsumePurchase(id, OnConsumePurchaseCompleted);
            }
        }

        private void OnGetCatalogCompleted(bool success, List<Dictionary<string, string>> catalog)
        {
            Debug.Log($"[YG-Bridge] OnGetCatalogCompleted, success: {success}, items:");

            if (!success)
                return;

            YG2.purchases = new Utils.Pay.Purchase[catalog.Count];

            var id = 0;
            foreach (var item in catalog)
            {
                Debug.Log("ID: " + item["id"]);
                Debug.Log("Price: " + item["price"]);
                Debug.Log("Price Currency Code: " + item["priceCurrencyCode"]);
                Debug.Log("Price Value: " + item["priceValue"]);

                YG2.purchases[id] = new Utils.Pay.Purchase
                {
                    id = item["id"],
                    price = item["price"],
                    priceValue = item["priceValue"],
                    priceCurrencyCode = item["priceCurrencyCode"]
                };

                id++;
            }

            YG2.langPayments = Bridge.platform.language;

            YG2.onGetPayments?.Invoke();
        }

        private void OnPurchaseCompleted(bool success, Dictionary<string, string> purchase)
        {
            string id = purchase != null && purchase.ContainsKey("id") ? purchase["id"] : "";
            Debug.Log($"[YG-Bridge] OnPurchaseCompleted, success: {success}, id: {id}");

            if (success)
            {
                YGInsides.OnPurchaseSuccess(id);
            }
            else
            {
                YGInsides.OnPurchaseFailed(id);
            }
        }

        private void OnConsumePurchaseCompleted(bool success, Dictionary<string, string> purchase)
        {
            string id = purchase != null && purchase.ContainsKey("id") ? purchase["id"] : "";
            Debug.Log($"[YG-Bridge] OnConsumePurchaseCompleted, success: {success}, id: {id}");
        }
    }
}
#endif
