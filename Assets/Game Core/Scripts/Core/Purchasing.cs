using UnityEngine;
using YG;
using System.Linq;
using Serbull.GameAssets;

public class Purchasing : MonoBehaviour
{
    [SerializeField] private Sprite _luckySpinIcon;

    private void Start()
    {
        YG2.onPurchaseSuccess += SuccessPurchased;
    }

    private void OnDestroy()
    {
        YG2.onPurchaseSuccess -= SuccessPurchased;
    }

    private void SuccessPurchased(string id)
    {
        switch(id)
        {
            case "spin_5":
                GameValues.AddLuckySpin(5);
                var luckySpinReward = new RewardPreviewItem("", "", _luckySpinIcon, 5, true, Color.white, Color.white, Color.white);
              Services.UI.RewardPreviewPopup.Show(luckySpinReward);
                return;
        }

        Debug.LogError("Not exist method for: " + id);
    }
}
