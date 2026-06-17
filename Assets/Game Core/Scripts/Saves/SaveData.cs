using System.Collections.Generic;
using Serbull.GameAssets.Roulette;

namespace YG
{
    public partial class SavesYG
    {
        public long coins;

        public int level;
        public int luckySpin = 1;
        public RouletteData Roulette = new();

        public List<bool> skinsPurchased = new();
        public List<bool> skinsEquipped = new();

        public int playtime;
        public int wins;
    }
}