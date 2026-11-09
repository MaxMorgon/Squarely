using TMPro;
using UnityEngine;

namespace Squarely.Presentation
{
    /// <summary>
    /// Shows the game's score and the current combo. The combo is shown only while it is at least <see cref="MinShownCombo"/>.
    /// </summary>
    public sealed class ScoreView : MonoBehaviour
    {
        /// <summary>The lowest combo that is shown. A combo of 1 is a single clear, not a streak yet.</summary>
        public const int MinShownCombo = 2;

        [SerializeField] private TMP_Text _scoreLabel;
        [SerializeField] private TMP_Text _comboLabel;

        public void SetScore(int score, int combo)
        {
            _scoreLabel.text = score.ToString();

            bool showCombo = combo >= MinShownCombo;
            _comboLabel.gameObject.SetActive(showCombo);

            if (showCombo)
                _comboLabel.text = $"Combo ×{combo}";
        }
    }
}
