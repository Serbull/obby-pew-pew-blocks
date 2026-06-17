using TMPro;
using UnityEngine;
using UnityEngine.UI;
using YG;

namespace SMP
{
    public class LeaderboardLine : MonoBehaviour
    {
        private static readonly Color _colorGold = new(1f, 0.84f, 0f);
        private static readonly Color _colorSilver = new(0.75f, 0.75f, 0.75f);
        private static readonly Color _colorBronze = new(0.8f, 0.5f, 0.2f);
        private static readonly Color _colorGreen = new(0.2f, 0.9f, 0.3f);
        private static readonly Color _colorBlue = new(0.3f, 0.7f, 1f);

        [HideInInspector]
        public LBPlayerData data = new();

        public ImageLoadYG imageLoad;
        public Image backgroundImage;

        public TextMeshProUGUI rankText, nameText, scoreText;

        public void UpdateEntries()
        {
            if (rankText && data.rank != null) rankText.text = data.rank.ToString();
            if (nameText && data.name != null) nameText.text = data.name;
            if (scoreText && data.score != null) scoreText.text = data.score.ToString();

            if (imageLoad)
            {
                if (data.photoSprite)
                {
                    imageLoad.SetTexture(data.photoSprite.texture);
                }
                else if (data.photoUrl == null)
                {
                    imageLoad.ClearTexture();
                }
                else
                {
                    imageLoad.Load(data.photoUrl);
                }
            }

            if (backgroundImage)
            {
                if (data.currentPlayer)
                {
                    backgroundImage.color = _colorGreen;
                }
                else if (data.rank == "1")
                {
                    backgroundImage.color = _colorGold;
                }
                else if (data.rank == "2")
                {
                    backgroundImage.color = _colorSilver;
                }
                else if (data.rank == "3")
                {
                    backgroundImage.color = _colorBronze;
                }
                else
                {
                    backgroundImage.color = _colorBlue;
                }
            }
        }
    }
}