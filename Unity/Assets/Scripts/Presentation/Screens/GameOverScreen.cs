using System;
using Squarely.Simulation;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Squarely.Presentation
{
    /// <summary>
    /// Shown over the game when it resolves: the result, the final score, and what to do next.
    /// Reports the player's choice; whoever owns it decides what happens next.
    /// </summary>
    public sealed class GameOverScreen : MonoBehaviour
    {
        [SerializeField] private TMP_Text _titleLabel;
        [SerializeField] private TMP_Text _scoreLabel;
        [SerializeField] private Button _restartButton;
        [SerializeField] private TMP_Text _restartLabel;
        [SerializeField] private Button _menuButton;
        [SerializeField] private TMP_Text _menuLabel;

        private GameOverTexts _texts;

        /// <summary>The player asked to play again with the same setup.</summary>
        public event Action RestartRequested;

        /// <summary>The player asked to go back to the menu.</summary>
        public event Action MenuRequested;

        #region Unity

        private void Awake()
        {
            _restartButton.onClick.AddListener(() => RestartRequested?.Invoke());
            _menuButton.onClick.AddListener(() => MenuRequested?.Invoke());
        }

        #endregion

        public void Initialize(GameOverTexts texts)
        {
            _texts = texts;
            _restartLabel.text = texts.Restart;
            _menuLabel.text = texts.Menu;
        }

        public void SetResult(GameResolution resolution, int finalScore)
        {
            _titleLabel.text = resolution.Outcome == GameOutcome.Won ? _texts.WinTitle : _texts.LoseTitle;
            _scoreLabel.text = string.Format(_texts.ScoreFormat, finalScore);
        }
    }
}
