using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Serbull.GameAssets;

public class DailyRewardSlot : MonoBehaviour
{
    public static event Action OnClaimed;

    [SerializeField] private Image _icon;
    [SerializeField] private TextMeshProUGUI _dayTxt;
    [SerializeField] private TextMeshProUGUI _countTxt;
    [SerializeField] private Button _button;
    [SerializeField] private GameObject _doneMark;
    [SerializeField] private GameObject _lockMark;

    private int _index;
    private DailyRewardConfig.Reward _reward;

    private void Awake()
    {
        _button.onClick.AddListener(Button_OnClick);
    }

    public void Init(DailyRewardConfig.Reward reward, int index, bool isAvailable, bool isClaimed)
    {
        _index = index;
        _reward = reward;

        if (_icon != null) _icon.sprite = reward.Icon;
        if (_countTxt != null) _countTxt.text = "x" + reward.Coins;
        if (_dayTxt != null) _dayTxt.text = $"{Services.Localization.GetText("day")} {(index + 1).ToString()}";
        UpdateState(isAvailable, isClaimed);
    }

    private void UpdateState(bool isAvailable, bool isClaimed)
    {
        if (_doneMark != null) _doneMark.SetActive(isClaimed);
        if (_lockMark != null) _lockMark.SetActive(!isAvailable);

        _button.gameObject.SetActive(isAvailable && !isClaimed);
    }

    private void Button_OnClick()
    {
        var rewards = SaveManager.Data.dailyReward;
        var claimed = SaveManager.Data.dailyRewardClimed;

        if (_index < 0 || _index >= claimed.Count || _index >= rewards.Count)
            return;

        if (!rewards[_index] || claimed[_index])
            return;

        claimed[_index] = true;

        ShopManager.Instance.AddCoins(_reward.Coins);

        SaveManager.SaveGameData();

        var item = new RewardPreviewItem("", "", _reward.Icon, _reward.Coins, true, Color.white, Color.white, Color.white);
        Services.UI.RewardPreviewPopup.Show(item);

        UpdateState(true, true);
        OnClaimed?.Invoke();
    }
}
