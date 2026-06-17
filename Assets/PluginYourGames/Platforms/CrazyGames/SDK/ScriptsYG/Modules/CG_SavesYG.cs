#if PaymentsXsolla_yg
using System;
using System.Collections.Generic;

namespace YG
{
    public partial class SavesYG
    {
        public List<GrantedPurchaseEntry> grantedPurchases = new();
    }

    [Serializable]
    public class GrantedPurchaseEntry
    {
        public string sku;
        public int quantity;
    }
}
#endif