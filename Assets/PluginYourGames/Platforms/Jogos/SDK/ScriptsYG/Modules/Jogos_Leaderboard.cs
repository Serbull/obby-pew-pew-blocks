#if JogosPlatform_yg
using UnityEngine;
using JogosGames.Engine.SDK;

namespace YG
{
    public partial class PlatformYG2 : IPlatformsYG2
    {
        public void SetLeaderboard(string nameLB, int score, string extraData)
        {
            JogosSDK.Game.CommitRankingData(nameLB, score.ToString(), (success) =>
            {
                Debug.Log("CommitRankingData is success: " + success);
            });
        }
    }
}
#endif
