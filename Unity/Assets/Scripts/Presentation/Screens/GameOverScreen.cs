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

        [Tooltip("Plays when this game set a new high score. Hang the celebration (animation, particles) here. No sound: GameState plays it.")]
        [SerializeField] private UnityEvent _newHighScoreCelebration = new UnityEvent();

        [SerializeField] private Button _restartButton;
        [SerializeField] private TMP_Text _restartLabel;
        [SerializeField] private Button _menuButton;
        [SerializeField] private TMP_Text _menuLabel;

        [Tooltip("Dev tools only: hidden until SetReplay runs.")]
        [SerializeField] private Button _copyReplayButton;
        [SerializeField] private TMP_Text _copyReplayLabel;

        [Tooltip("Inactive by default. Shows briefly after a copy.")]
        [SerializeField] private TMP_Text _copiedLabel;

        private GameOverTexts _texts;

#if UNITY_EDITOR || SQUARELY_DEV_TOOLS
        private const float CopiedSeconds = 1.5f;

        private ReplayCopier _copier;
        private GameReplay _replay;
        private float _hideCopiedTime;
#endif

        /// <summary>The player asked to play again with the same setup.</summary>
        public event Action RestartRequestEvent;

        /// <summary>The player asked to go back to the menu.</summary>
        public event Action MenuRequestEvent;

        #region Unity

        private void Awake()
        {
            _restartButton.onClick.AddListener(() => RestartRequestEvent?.Invoke());
            _menuButton.onClick.AddListener(() => MenuRequestEvent?.Invoke());

#if UNITY_EDITOR || SQUARELY_DEV_TOOLS
            _copyReplayButton.onClick.AddListener(CopyReplay);
#endif
            _copyReplayButton.gameObject.SetActive(false);
            _copiedLabel.gameObject.SetActive(false);
        }

#if UNITY_EDITOR || SQUARELY_DEV_TOOLS
        private void Update()
        {
            if (_copiedLabel.gameObject.activeSelf && Time.unscaledTime >= _hideCopiedTime)
                _copiedLabel.gameObject.SetActive(false);
        }
#endif

        #endregion

        public void Initialize(GameOverTexts texts)
        {
            _texts = texts;
            _restartLabel.text = texts.Restart;
            _menuLabel.text = texts.Menu;
            _copyReplayLabel.text = texts.CopyReplay;
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

#if UNITY_EDITOR || SQUARELY_DEV_TOOLS
        /// <summary>Shows the Copy replay button. It copies the given replay with the given copier.</summary>
        public void SetReplay(ReplayCopier copier, GameReplay replay)
        {
            _copier = copier;
            _replay = replay;
            _copyReplayButton.gameObject.SetActive(true);
        }

        private void CopyReplay()
        {
            if (_copier == null)
                return;

            _copier.Copy(_replay);
            _copiedLabel.text = _texts.ReplayCopied;
            _copiedLabel.gameObject.SetActive(true);
            _hideCopiedTime = Time.unscaledTime + CopiedSeconds;
        }
#endif

        private void CelebrateNewHighScore()
        {
            _newHighScoreCelebration.Invoke();
        }
    }
}
