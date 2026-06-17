using UnityEngine;
using UnityEngine.UI;
using YG;

[RequireComponent(typeof(LeaderboardYG))]
public class OpenLeaderboardDialog : MonoBehaviour
{
    public Button button;

    private LeaderboardYG _lb;

    private void Start()
    {
#if JogosPlatform_yg
        button.gameObject.SetActive(true);
        button.onClick.AddListener(OnClick);
        _lb = GetComponent<LeaderboardYG>();
#else
        button.gameObject.SetActive(false);
#endif
    }

    private void OnClick()
    {
#if JogosPlatform_yg
        JogosGames.Engine.SDK.JogosSDK.Game.OpenRankingDialog(_lb.nameLB, (success) =>
        {
            Debug.Log("OpenRankingDialog is success: " + success);
        });
#endif
    }
}
