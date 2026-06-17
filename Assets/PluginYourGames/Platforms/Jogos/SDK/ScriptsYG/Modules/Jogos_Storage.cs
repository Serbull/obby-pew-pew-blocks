#if JogosPlatform_yg && Storage_yg
using JogosGames.Engine.SDK;
using UnityEngine;
using YG.Insides;
#if NJSON_STORAGE_YG2
using Newtonsoft.Json;
#endif

namespace YG
{
    public partial class PlatformYG2 : IPlatformsYG2
    {
        private const string KEY_SAVE = "Jogos_SavesYG";

        public void SynchronizeToCloud()
        {
            JogosSDK.Data.SynchronizeToCloud();
        }

        public void LoadCloud()
        {
            string strSaves = PlayerPrefs.GetString(KEY_SAVE, null);
            YGInsides.SetLoadSaves(strSaves);
        }

        public void SaveCloud()
        {
#if NJSON_STORAGE_YG2
            PlayerPrefs.SetString(KEY_SAVE, JsonConvert.SerializeObject(YG2.saves));
#else
            PlayerPrefs.SetString(KEY_SAVE, JsonUtility.ToJson(YG2.saves));
#endif
            SynchronizeToCloud();
        }
    }
}
#endif