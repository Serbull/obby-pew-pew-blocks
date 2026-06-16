using System.Collections.Generic;
using Serbull.GameAssets.Pets;
using Serbull.GameAssets.Roulette;

namespace YG
{
    public partial class SavesYG
    {
        public long coins;

        public int level;
        public int luckySpin = 1;

        public List<PetData> Pets = new();
        public RouletteData Roulette = new();

        public List<bool> skinsPurchased = new();
        public List<bool> skinsEquipped = new();
    }
}