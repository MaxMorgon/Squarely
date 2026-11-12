using System;
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

        /// <summary>The score the label shows.</summary>
        public int Score { get; private set; }

        /// <summary>The combo last set, also when it is below the shown minimum.</summary>
        public int Combo { get; private set; }

        public Transform ScoreLabel => _scoreLabel.transform;

        public Transform ComboLabel => _comboLabel.transform;

        /// <summary>The score the label shows changed, also at each step of a count-up. Gives the new score.</summary>
        public event Action<int> ScoreChangeEvent;

        /// <summary>Starts at a score of 0 and no combo.</summary>
        public void Initialize(GameHudTexts texts)
        {
            _texts = texts;
            SetScore(0);
            SetCombo(0);
        }

        public void SetScore(int score)
        {
            bool isChanged = score != Score;
            Score = score;
            _scoreLabel.text = score.ToString();

            if (isChanged)
                ScoreChangeEvent?.Invoke(score);
        }

        public void SetCombo(int combo)
        {
            Combo = combo;

            bool showCombo = combo >= _minShownCombo;
            _comboLabel.gameObject.SetActive(showCombo);

            if (showCombo)
                _comboLabel.text = string.Format(_texts.ComboFormat, combo);
        }
    }
}
