using System;
using System.Collections.Generic;
using Serbull.GameAssets.Pets;
using Serbull.GameAssets.Roulette;

namespace YG
{
    public partial class SavesYG
    {
        public int sound = 1;
        public int music = 1;
        public float cameraSensitivity = 0.3f;

        public long coins;

        public int level;
        public int luckySpin = 1;

        public List<PetData> Pets;
        public RouletteData Roulette;
    }
}