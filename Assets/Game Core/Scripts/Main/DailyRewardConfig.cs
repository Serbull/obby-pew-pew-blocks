using System;
using UnityEngine;

[CreateAssetMenu(fileName = "DailyRewardConfig", menuName = "Configs/Daily Reward Config")]
public class DailyRewardConfig : ScriptableObject
{
    [Serializable]
    public class Reward
    {
        public Sprite Icon;
        public int Coins;
    }

    public Reward[] Datas;

    private static DailyRewardConfig _instance;

    public static DailyRewardConfig Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = Resources.Load<DailyRewardConfig>("DailyRewardConfig");
            }

            return _instance;
        }
    }
}
