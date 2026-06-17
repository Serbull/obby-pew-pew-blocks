using UnityEngine;
using YG;
using TMPro;

namespace SMP
{
    public class PurchaseView : MonoBehaviour
    {
        public string id;
        public TextMeshProUGUI titleText, descriptionText, priceText;

        protected virtual void Start()
        {
            UpdateEntries(YG2.PurchaseByID(id));
        }

        public void UpdateEntries(YG.Utils.Pay.Purchase data)
        {
            if (data == null)
            {
                Debug.LogError($"No product with ID found: {id}");
                data = new YG.Utils.Pay.Purchase()
                {
                    id = id,
                    title = "--",
                    description = "--",
                    price = "--"
                };
            }

            if (titleText) titleText.text = data.title;
            if (descriptionText) descriptionText.text = data.description;
            if (priceText) priceText.text = data.price;
        }

        public void BuyPurchase()
        {
            YG2.BuyPayments(id);
        }
    }
}
