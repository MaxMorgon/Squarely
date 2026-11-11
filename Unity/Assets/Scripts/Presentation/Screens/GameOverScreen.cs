using System;
using Squarely.Simulation;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Squarely.Presentation
{
    /// <summary>
    /// Shown over the game when it resolves: the result, the final score, the high score, and what to do next.
    /// Reports the player's choice; whoever owns it decides what happens next.
    /// </summary>
    public sealed class GameOverScreen : MonoBehaviour
    {
        [SerializeField] private TMP_Text _titleLabel;
        [SerializeField] private TMP_Text _scoreLabel;

        [Tooltip("Shows the high score when this game did not beat it.")]
        [SerializeField] private TMP_Text _highScoreLabel;

        [Tooltip("Shown instead of the high score when this game set a new one.")]
        [SerializeField] private TMP_Text _newHighScoreLabel;

        [Tooltip("Plays when this game set a new high score. Hang the celebration (animation, particles, sound) here.")]
        [SerializeField] private UnityEvent _newHighScoreCelebration = new UnityEvent();

        [SerializeField] private Button _restartButton;
        [SerializeField] private TMP_Text _restartLabel;
        [SerializeField] private Button _menuButton;
        [SerializeField] private TMP_Text _menuLabel;

        private GameOverTexts _texts;

        /// <summary>The player asked to play again with the same setup.</summary>
        public event Action RestartRequestEvent;

        /// <summary>The player asked to go back to the menu.</summary>
        public event Action MenuRequestEvent;

        #region Unity

        private void Awake()
        {
            _restartButton.onClick.AddListener(() => RestartRequestEvent?.Invoke());
            _menuButton.onClick.AddListener(() => MenuRequestEvent?.Invoke());
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

        /// <summary>
        /// Sets the high score shown. A new one shows the "new high score" state and plays its celebration;
        /// otherwise the label shows the high score this game did not beat.
        /// </summary>
        public void SetHighScore(int highScore, bool isNew)
        {
            _newHighScoreLabel.gameObject.SetActive(isNew);
            _highScoreLabel.gameObject.SetActive(!isNew);

            if (isNew)
            {
                _newHighScoreLabel.text = _texts.NewHighScore;
                CelebrateNewHighScore();
            }
            else
            {
                _highScoreLabel.text = string.Format(_texts.HighScoreFormat, highScore);
            }
        }

        private void CelebrateNewHighScore()
        {
            _newHighScoreCelebration.Invoke();
        }
    }
}
