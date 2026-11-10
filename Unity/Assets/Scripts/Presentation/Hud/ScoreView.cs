using TMPro;
using UnityEngine;

namespace Squarely.Presentation
{
    /// <summary>
    /// Shows the game's score and the current combo. The combo is shown only from a configured minimum up.
    /// </summary>
    public sealed class ScoreView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _scoreLabel;
        [SerializeField] private TMP_Text _comboLabel;

        [Tooltip("The lowest combo that is shown. A combo of 1 is a single clear, not a streak yet.")]
        [SerializeField] private int _minShownCombo = 2;

        private GameHudTexts _texts;

        public void Initialize(GameHudTexts texts)
        {
            _texts = texts;
        }

        public void SetScore(int score, int combo)
        {
            _scoreLabel.text = score.ToString();

            bool showCombo = combo >= _minShownCombo;
            _comboLabel.gameObject.SetActive(showCombo);

            if (showCombo)
                _comboLabel.text = string.Format(_texts.ComboFormat, combo);
        }
    }
}
