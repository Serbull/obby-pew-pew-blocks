using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using YG;
using YG.Utils.LB;
using System.Collections.Generic;
using System;
#if BridgePlatform_yg
using Playgama;
#endif

namespace SMP
{
    public class Leaderboard : MonoBehaviour
    {
        public string nameLB;
        public int maxQuantityPlayers = 13;
        public int quantityTop = 10;
        public int quantityAround = 1;

        public enum UpdateLBMethod { Start, OnEnable, DoNotUpdate };
        [Space]
        public UpdateLBMethod updateLBMethod = UpdateLBMethod.OnEnable;
        [Min(0)] public int updateTime = 30;
        [Space]

        public Transform lineRootSpawn;
        public LeaderboardLine linePrefab;
        public Button nativePopupButton;

        public enum PlayerPhoto { NonePhoto, Small, Medium, Large };
        public PlayerPhoto playerPhoto = PlayerPhoto.Small;
        public Sprite isHiddenPlayerPhoto;
        public bool timeTypeConvert;
#if UNITY_EDITOR
        [NestedYG("timeTypeConvert"), Range(0, 3)]
#endif
        public int decimalSize = 0;

        public UnityEvent onUpdateData;

        private LeaderboardLine[] lines = new LeaderboardLine[0];

        private float _lastUpdateTime = float.MinValue;

        private void OnEnable()
        {
            YG2.onGetLeaderboard += OnUpdateLB;

            if (updateLBMethod == UpdateLBMethod.OnEnable)
                UpdateLB();
        }
        private void OnDisable()
        {
            YG2.onGetLeaderboard -= OnUpdateLB;
        }

        private IEnumerator Start()
        {
            if (nativePopupButton)
            {
                nativePopupButton.gameObject.SetActive(false);
                nativePopupButton.onClick.AddListener(ShowNativePopup);
            }

#if !UNITY_EDITOR && BridgePlatform_yg
            Debug.Log("Leaderboard type: " + Bridge.leaderboards.type);

            switch (Bridge.leaderboards.type)
            {
                case Playgama.Modules.Leaderboards.LeaderboardType.InGame:
                    break;
                case Playgama.Modules.Leaderboards.LeaderboardType.NativePopup:
                    if (nativePopupButton)
                    {
                        nativePopupButton.gameObject.SetActive(true);
                    }
                    else
                    {
                        Debug.LogError("nativePopupButton is null");
                    }
                    yield break;
                default:
                    gameObject.SetActive(false);
                    yield break;
            }
#endif

            if (updateLBMethod == UpdateLBMethod.Start)
                UpdateLB();

            while (updateTime > 0)
            {
                yield return new WaitForSeconds(updateTime);
                UpdateLB();
            }
        }

        private void OnUpdateLB(LBData lbData)
        {
            if (lbData.technoName != nameLB)
                return;

            string noData = string.Empty;
#if Localization_yg
            if (lbData.entries == InfoYG.NO_DATA)
            {
                noData = YG2.lang switch
                {
                    "ru" => "Нет данных",
                    "en" => "No data",
                    "tr" => "Veri yok",
                    _ => string.Empty,
                };
            }
#endif

            DestroyLBList();

            if (lbData.entries == InfoYG.NO_DATA)
            {
                lines = new LeaderboardLine[1];
                lines[0] = Instantiate(linePrefab, lineRootSpawn);
                lines[0].data.name = noData;
                lines[0].data.photoUrl = null;
                lines[0].data.rank = null;
                lines[0].data.score = null;
                lines[0].data.inTop = false;
                lines[0].data.currentPlayer = false;
                lines[0].data.photoSprite = null;
                lines[0].UpdateEntries();
            }
            else
            {
#if UNITY_EDITOR
                lbData = LBMethods.SortLB(lbData, maxQuantityPlayers, quantityTop, quantityAround);
#else
                if (lbData.players.Length > maxQuantityPlayers)
                {
                    int currentPlayer = -1;

                    for (int i = 0; i < lbData.players.Length; i++)
                    {
                        if (lbData.players[i].uniqueID == YG2.player.id)
                        {
                            currentPlayer = i;
                            break;
                        }
                    }

                    if (currentPlayer >= maxQuantityPlayers)
                    {
                        List<YG.Utils.LB.LBPlayerData> topPlayers = new();

                        for (int i = 0; i < quantityTop; i++)
                            topPlayers.Add(lbData.players[i]);

                        int minusPlayers = lbData.players.Length - maxQuantityPlayers;
                        List<YG.Utils.LB.LBPlayerData> otherPlayers = new();

                        for (int i = quantityTop + minusPlayers; i < lbData.players.Length; i++)
                            otherPlayers.Add(lbData.players[i]);

                        List<YG.Utils.LB.LBPlayerData> finalPlayers = topPlayers;
                        finalPlayers.AddRange(otherPlayers);

                        lbData.players = finalPlayers.ToArray();
                    }
                    else
                    {
                        Array.Resize(ref lbData.players, maxQuantityPlayers);
                    }
                }
#endif
                SpawnPlayersList(lbData);
            }

            onUpdateData?.Invoke();
        }

        private void DestroyLBList()
        {
            int childCount = lineRootSpawn.childCount;
            for (int i = childCount - 1; i >= 0; i--)
            {
                Destroy(lineRootSpawn.GetChild(i).gameObject);
            }
        }

        private void SpawnPlayersList(LBData lb)
        {
            lines = new LeaderboardLine[lb.players.Length];

            for (int i = 0; i < lines.Length; i++)
            {
                lines[i] = Instantiate(linePrefab, lineRootSpawn);

                int rank = lb.players[i].rank;

                lines[i].data.name = LBMethods.AnonymousName(lb.players[i].name);
                lines[i].data.rank = rank.ToString();

                if (rank <= 3)
                {
                    lines[i].data.inTop = true;
                }
                else
                {
                    lines[i].data.inTop = false;
                }

                if (lb.players[i].uniqueID == YG2.player.id)
                {
                    lines[i].data.currentPlayer = true;
                }
                else
                {
                    lines[i].data.currentPlayer = false;
                }

                if (timeTypeConvert)
                {
                    string timeScore = TimeTypeConvert(lb.players[i].score);
                    lines[i].data.score = timeScore;
                }
                else
                {
                    lines[i].data.score = lb.players[i].score.ToString();
                }

                if (playerPhoto != PlayerPhoto.NonePhoto)
                {
                    if (isHiddenPlayerPhoto
                        && (lb.players[i].photo.Contains("/avatar/0/")
                        || lb.players[i].photo == InfoYG.ANONYMOUS
                        || string.IsNullOrEmpty(lb.players[i].photo)))
                    {
                        lines[i].data.photoSprite = isHiddenPlayerPhoto;
                    }
                    else
                    {
                        lines[i].data.photoUrl = lb.players[i].photo;
                    }
                }

                lines[i].UpdateEntries();
            }
        }

        public void UpdateLB()
        {
            if (Time.time - _lastUpdateTime < 10f)
                return;

            string photoSize = "nonePhoto";

            switch (playerPhoto)
            {
                case PlayerPhoto.Small:
                    photoSize = "small";
                    break;
                case PlayerPhoto.Medium:
                    photoSize = "medium";
                    break;
                case PlayerPhoto.Large:
                    photoSize = "large";
                    break;
            }

            YG2.GetLeaderboard(nameLB, quantityTop, quantityAround, photoSize);
            _lastUpdateTime = Time.time;
        }

        public string TimeTypeConvert(int score)
        {
            return LBMethods.TimeTypeConvertStatic(score, decimalSize);
        }

        private void ShowNativePopup()
        {
            Debug.Log("Show native leaderboard");
#if BridgePlatform_yg
            Bridge.leaderboards.ShowNativePopup(nameLB);
#endif
        }
    }
}
