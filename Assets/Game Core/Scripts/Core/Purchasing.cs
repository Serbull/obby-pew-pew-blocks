using UnityEngine;
using YG;
using System.Linq;
using Serbull.GameAssets;
using Serbull.GameAssets.Pets;

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
            case "pet_1":
            case "pet_2":
            case "pet_3":
            case "pet_4":
            case "pet_5":
            case "pet_6":
            case "pet_7":
                var triggers = FindObjectsByType<InAppPetStandTrigger>(FindObjectsSortMode.None);
                var trigger = triggers.FirstOrDefault(i => i.InappId == id);
                var petId = trigger.GetComponent<InAppPetStand>().PetId;
                PetManager.AddPet(petId);
                PetManager.PreviewPet(petId);
                return;
            case "spin_5":
                GameValues.AddLuckySpin(5);
                var luckySpinReward = new RewardPreviewItem("", "", _luckySpinIcon, 5, true, Color.white, Color.white, Color.white);
              Services.UI.RewardPreviewPopup.Show(luckySpinReward);
                return;
        }

        Debug.LogError("Not exist method for: " + id);
    }
}
