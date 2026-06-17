#if BridgePlatform_yg && Storage_yg
using UnityEngine;
using System.Collections;
using YG.Insides;
using Playgama;

using System.Collections.Generic;
using YG.Utils.LB;

namespace YG
{
    public partial class PlatformYG2 : IPlatformsYG2
    {
        private readonly Queue<string> _lbQueue = new();
        private bool _lbRequestInProgress;
        private string _lastNameLB;
        private static readonly WaitForSeconds _lbRequestDelay = new(2f);

        public void SetLeaderboard(string nameLB, int score, string extraData)
        {
            Bridge.leaderboards.SetScore(nameLB, score, OnSetScoreCompleted);
        }

        public void GetLeaderboard(string nameLB, int quantityTop, int quantityAround, string photoSizeLB)
        {
            if (!YG2.infoYG.Leaderboards.enable)
            {
                YG2.onGetLeaderboard?.Invoke(YGInsides.NoLBData(nameLB));
                return;
            }

            _lbQueue.Enqueue(nameLB);
            TryRequestLeaderboard();
        }

        private void TryRequestLeaderboard()
        {
            YG2.Message("Try request leaderboard.");

            if (_lbRequestInProgress || _lbQueue.Count == 0)
                return;

            _lbRequestInProgress = true;
            _lastNameLB = _lbQueue.Dequeue();

            YG2.Message($"Get Leaderboard: {_lastNameLB}");
            Bridge.leaderboards.GetEntries(_lastNameLB, OnGetEntriesCompleted);
        }

        private void OnSetScoreCompleted(bool success)
        {
            if (success)
            {
                Debug.Log("Set leaderboard completed.");
            }
            else
            {
                Debug.LogWarning("Set leaderboard not completed.");
            }
        }

        private void OnGetEntriesCompleted(bool success, List<Dictionary<string, string>> entries)
        {
            YG2.Message($"Get leaderboard completed: {success}");

            if (!success)
            {
                YG2.sendMessage.StartCoroutine(DelayedTryRequestLeaderboard());
                return;
            }

            string playerId = Bridge.player.id;

            LBData lbData = new()
            {
                technoName = _lastNameLB,
                players = new LBPlayerData[entries.Count],
                currentPlayer = null
            };

            for (int i = 0; i < entries.Count; i++)
            {
                var entry = entries[i];
                int.TryParse(entry["rank"], out int rank);
                int.TryParse(entry["score"], out int score);

                lbData.players[i] = new LBPlayerData
                {
                    name = entry["name"],
                    rank = rank,
                    score = score,
                    photo = entry["photo"],
                    uniqueID = entry["id"],
                    extraData = null
                };

                if (lbData.players[i].uniqueID == playerId)
                {
                    lbData.currentPlayer = new LBCurrentPlayerData
                    {
                        rank = lbData.players[i].rank,
                        score = lbData.players[i].score,
                        extraData = lbData.players[i].extraData
                    };
                }
            }

            YG2.onGetLeaderboard?.Invoke(lbData);
            YG2.sendMessage.StartCoroutine(DelayedTryRequestLeaderboard());
        }

        private IEnumerator DelayedTryRequestLeaderboard()
        {
            yield return _lbRequestDelay;
            _lbRequestInProgress = false;
            TryRequestLeaderboard();
        }
    }
}
#endif
