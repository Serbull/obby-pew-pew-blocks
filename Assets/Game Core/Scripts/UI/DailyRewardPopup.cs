using UnityEngine;

public class DailyRewardPopup : MonoBehaviour
{
    [SerializeField] private DailyRewardSlot[] _giftSlot;

    private void OnEnable()
    {
        Init();
    }

    public void Init()
    {
        var config = DailyRewardConfig.Instance;
        var rewards = SaveManager.Data.dailyReward;
        var claimed = SaveManager.Data.dailyRewardClimed;

        for (int i = 0; i < _giftSlot.Length; i++)
        {
            if (config == null || i >= config.Datas.Length)
                continue;

            bool isAvailable = rewards != null && i < rewards.Count && rewards[i];
            bool isClaimed = claimed != null && i < claimed.Count && claimed[i];

            _giftSlot[i].Init(config.Datas[i], i, isAvailable, isClaimed);
        }
    }
}
