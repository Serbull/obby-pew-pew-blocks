#if CrazyGamesPlatform_yg && PaymentsXsolla_yg
using System;
using System.Collections;
using CrazyGames;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;
using Xsolla.Catalog;
using Xsolla.Core;
using YG.Insides;

namespace YG
{
    public partial class PlatformYG2 : IPlatformsYG2
    {
        private static readonly WaitForSeconds _waitForSeconds0_5 = new(0.5f);

        [Serializable]
        public class XsollaInventoryResponse
        {
            public XsollaInventoryItem[] items;
        }

        [Serializable]
        public class XsollaInventoryItem
        {
            public string sku;
            public string virtual_item_type;
            public int quantity;
        }

        private static string _userToken;
        private static float _tokenLastFetchedTime = float.MinValue;
        private static float _inventoryLastFetchedTime = float.MinValue;

        public void InitPayments()
        {
            XsollaCatalog.GetItems((catalog) =>
            {
                Debug.Log($"[CG Xsolla] {catalog.items.Length} items received");

                YG2.purchases = new Utils.Pay.Purchase[catalog.items.Length];

                for (int i = 0; i < catalog.items.Length; i++)
                {
                    var item = catalog.items[i];

                    Debug.Log("ID: " + item.sku);
                    Debug.Log("Title: " + item.name);
                    Debug.Log("Description: " + item.description);
                    Debug.Log("Image URI: " + item.image_url);
                    Debug.Log("Price: " + item.price.amount);
                    Debug.Log("Currency: " + item.price.currency);

                    YG2.purchases[i] = new Utils.Pay.Purchase
                    {
                        id = item.sku,
                        title = item.name,
                        description = item.description,
                        imageURI = item.image_url,
                        price = $"{item.price.amount} {item.price.currency}",
                        priceValue = item.price.amount,
                        priceCurrencyCode = item.price.currency
                    };
                }

                YG2.onGetPayments?.Invoke();

            }, (error) =>
            {
                Debug.LogError($"[CG Xsolla] Error fetch items: {error.errorMessage}");
            });
        }

        public void BuyPayments(string id)
        {
            YG2.PauseGame(false);

            if (!YG2.player.auth)
            {
                YG2.OpenAuthDialog();
                return;
            }

            PurchaseTooltip.Show();

            GetXsollaUserToken(() =>
            {
                XsollaCatalog.Purchase(id, orderStatus =>
                    {
                        Debug.Log($"[CG Xsolla] Order status: {orderStatus.status}");
                        CrazySDK.Analytics.TrackOrder(PaymentProvider.Xsolla, orderStatus);
                        YGInsides.OnPurchaseSuccess(id);
                    }, error =>
                    {
                        Debug.LogError($"[CG Xsolla] Failed to buy, Error: {error.errorMessage}");
                        YGInsides.OnPurchaseFailed(id);
                    });
            }, null);
        }

        public void ConsumePurchases(bool onPurchaseSuccess)
        {
            GetXsollaUserToken(() => YG2.sendMessage.StartCoroutine(FetchInventory()), null);
        }

        public static void TryFetchInventory()
        {
            if (Time.unscaledTime - _inventoryLastFetchedTime < 20) return;

            GetXsollaUserToken(() => YG2.sendMessage.StartCoroutine(FetchInventory()), null);
        }

        private static void GetXsollaUserToken(Action onOk, Action onFail)
        {
            if (!string.IsNullOrEmpty(_userToken) && Time.unscaledTime - _tokenLastFetchedTime < 300)
            {
                onOk?.Invoke();
                return;
            }

            Debug.Log("[CG Xsolla] Requesting user token...");

            CrazySDK.User.GetXsollaUserToken((error, token) =>
            {
                if (error != null)
                {
                    Debug.LogError("[CG Xsolla] Get user token error: " + error);
                    onFail?.Invoke();
                }
                else
                {
                    Debug.Log("[CG Xsolla] User token successful.");
                    _tokenLastFetchedTime = Time.unscaledTime;
                    XsollaToken.Create(token);
                    _userToken = token;
                    onOk?.Invoke();
                }
            });
        }

        private static IEnumerator FetchInventory()
        {
#if UNITY_EDITOR
            yield break;
#endif

            _inventoryLastFetchedTime = Time.unscaledTime;

            Debug.Log("[CG Xsolla] Inventory fetching user...");

            var url = $"https://store.xsolla.com/api/v2/project/{XsollaSettings.StoreProjectId}/user/inventory/items";

            using var req = UnityWebRequest.Get(url);
            req.SetRequestHeader("Authorization", $"Bearer {_userToken}");

            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"[CG Xsolla] Inventory error: {req.responseCode} {req.error}\n{req.downloadHandler.text}");
                yield break;
            }

            var json = req.downloadHandler.text;
            Debug.Log("[CG Xsolla] Inventory JSON: " + json);

            var response = JsonConvert.DeserializeObject<XsollaInventoryResponse>(json);

            yield return _waitForSeconds0_5;

            CheckInventory(response);
        }

        private static void CheckInventory(XsollaInventoryResponse response)
        {
            bool saveNeeded = false;

            foreach (var item in response.items)
            {
                var granted = YG2.saves.grantedPurchases.Find(g => g.sku == item.sku);
                int alreadyGranted = granted?.quantity ?? 0;
                int delta = item.quantity - alreadyGranted;

                if (delta <= 0) continue;

                for (int i = 0; i < delta; i++)
                    YGInsides.OnPurchaseSuccess(item.sku);

                if (granted == null)
                    YG2.saves.grantedPurchases.Add(new GrantedPurchaseEntry { sku = item.sku, quantity = item.quantity });
                else
                    granted.quantity = item.quantity;

                saveNeeded = true;
            }

            if (saveNeeded)
                YG2.SaveProgress();
        }
    }
}
#endif
