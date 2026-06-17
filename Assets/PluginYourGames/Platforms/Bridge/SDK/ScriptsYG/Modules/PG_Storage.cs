#if BridgePlatform_yg && Storage_yg
using UnityEngine;
using YG.Insides;
using Playgama;
#if NJSON_STORAGE_YG2
using Newtonsoft.Json;
#endif

namespace YG
{
    public partial class PlatformYG2 : IPlatformsYG2
    {
        private const string KEY_SAVE = "PG_SavesYG";

        private static string saveData;

        public void LoadAndCacheCloud(System.Action onCompleted)
        {
            Bridge.storage.Get(KEY_SAVE, (success, data) =>
            {
                if (success)
                {
                    saveData = data;
                }

                onCompleted?.Invoke();
            });
        }

        public void LoadCloud()
        {
            YGInsides.SetLoadSaves(saveData);
        }

        public void SaveCloud()
        {
#if NJSON_STORAGE_YG2
            Bridge.storage.Set(KEY_SAVE, JsonConvert.SerializeObject(YG2.saves), null);
#else
            Bridge.storage.Set(KEY_SAVE, JsonUtility.ToJson(YG2.saves), null);
#endif
        }
    }
}
#endif