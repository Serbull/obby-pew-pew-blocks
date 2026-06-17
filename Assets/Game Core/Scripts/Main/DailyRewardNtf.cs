using System;
using UnityEngine;

public class DailyRewardNtf : MonoBehaviour
{
    [SerializeField] private GameObject _root;

    private const string DateFormat = "yyyy-MM-dd";

    private void OnEnable()
    {
        DailyRewardSlot.OnClaimed += GiftSlot_OnClaimed;
    }

    private void Start()
    {
        UpdateDailyByDate();  
        Refresh();            
    }

    private void OnDisable()
    {
        DailyRewardSlot.OnClaimed -= GiftSlot_OnClaimed;
    }

    private void GiftSlot_OnClaimed()
    {
        Refresh();
    }

    private void Refresh()
    {
        _root.SetActive(HasRewardToClaim());
    }

    private void UpdateDailyByDate()
    {
        string today = DateTime.Now.Date.ToString(DateFormat);
        string saved = SaveManager.Data.lastLoginDate;

        // первый запуск
        if (string.IsNullOrEmpty(saved))
        {
            SaveManager.Data.lastLoginDate = today;
            EnsureLists();
            SaveManager.Data.dailyReward[0] = true;
            
            return;
        }

        // тот же день
        if (saved == today)
            return;

        // новый день
        EnsureLists();

        int nextIndex = GetNextIndex();
        SaveManager.Data.dailyReward[nextIndex] = true;
        SaveManager.Data.dailyRewardClimed[nextIndex] = false;

        SaveManager.Data.lastLoginDate = today; 
    }

    private void EnsureLists()
    {
        if (SaveManager.Data.dailyReward == null)
            SaveManager.Data.dailyReward = new System.Collections.Generic.List<bool>();

        if (SaveManager.Data.dailyRewardClimed == null)
            SaveManager.Data.dailyRewardClimed = new System.Collections.Generic.List<bool>();

        while (SaveManager.Data.dailyReward.Count < 7) SaveManager.Data.dailyReward.Add(false);
        while (SaveManager.Data.dailyRewardClimed.Count < 7) SaveManager.Data.dailyRewardClimed.Add(false);
    }

    private int GetNextIndex()
    {
        var rewards = SaveManager.Data.dailyReward;

        for (int i = 0; i < rewards.Count; i++)
        {
            // следующий “день” — первый ещё НЕ открытый
            if (!rewards[i])
                return i;
        }

        // если все уже открыты — начинаем с 0 (без ресета)
        // (если хочешь ресетить 7-дневку — скажи, добавлю)
        return 0;
    }

    private bool HasRewardToClaim()
    {
        var rewards = SaveManager.Data.dailyReward;
        var claimed = SaveManager.Data.dailyRewardClimed;

        if (rewards == null || claimed == null)
            return false;

        int count = Mathf.Min(rewards.Count, claimed.Count);

        for (int i = 0; i < count; i++)
        {
            if (rewards[i] && !claimed[i])
                return true;
        }

        return false;
    }
}