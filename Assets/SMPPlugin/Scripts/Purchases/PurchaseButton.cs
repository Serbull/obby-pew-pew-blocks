using UnityEngine;
using UnityEngine.UI;

namespace SMP
{
    [RequireComponent(typeof(Button))]
    public sealed class PurchaseButton : PurchaseView
    {
        protected override void Start()
        {
            base.Start();

            GetComponent<Button>().onClick.AddListener(BuyPurchase);
        }
    }
}
