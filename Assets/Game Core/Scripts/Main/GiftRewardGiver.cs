using System.Collections.Generic;
using UnityEngine;

public class GiftRewardGiver
{
    //public void GiveReward(RecourceType recourceType, int value)
    //{
    //    Debug.LogError("NEED GiveReward LOGIC!!!");
    //   // switch (recourceType)
    //   // {
    //   //     case RecourceType.Cup:
    //   //      //   GameValues.AddCups(value);
    //   //         break;
    //   //     case RecourceType.Energy:
    //   //        // GameValues.AddEnergy(value);
    //   //         break;
    //   //     case RecourceType.LuckySpin:
    //   //         GameValues.AddLuckySpin(value);
    //   //         break;
    //   //     case RecourceType.LegendaryPet:
    //   //        // PetValues.AddPet("lizard");
    //   //         ShowPetPreview("lizard");
    //   //         break;
    //   //     case RecourceType.Egg1:
    //   //         AddEgg("egg_1");
    //   //         break;
    //   //     case RecourceType.Egg2:
    //   //         AddEgg("egg_2");
    //   //         break;
    //   //     case RecourceType.Egg3:
    //   //         AddEgg("egg_3");
    //   //         break;
    //   //     case RecourceType.MusticalPet:
    //   //         PetValues.AddPet("blue_dragon");
    //   //         ShowPetPreview("blue_dragon");
    //   //         break;
    //   //     default:
    //   //         break;
    //   // }
    //}

   // public void AddEgg(string id)
   // {
   //     if (Configs.Instance.PetConfig.IsInventoryFull())
   //     {
   //         return;
   //     }
   //
   //     var petConfig = Configs.Instance.PetConfig;
   //     var eggData = petConfig.GetEggData(id);
   //
   //     if (eggData == null)
   //     {
   //         Debug.LogWarning($"Egg data not found for ID: {id}");
   //         return;
   //     }
   //
   //     int probabilityIndex = MathfUtils.GetRandomIndexByWeight(petConfig.GetEggIndexes(id));
   //     var petId = eggData.PetPobabilities[probabilityIndex].PetId;
   //
   //     PetValues.AddPet(petId);
   //     ShowPetPreview(petId);
   // }
   //
   // public void ShowPetPreview(string petId)
   // {
   //     var petConfig = Configs.Instance.PetConfig;
   //     int petRare = petConfig.GetPetRare(petId);
   //     List<RewardSlotData> reward = new() { new RewardSlotData { Rare = petRare, Sprite = petConfig.GetPet(petId).Icon } };
   //     UIManager.Instance.RewardPreviewPopup.Show(reward);
   // }
}