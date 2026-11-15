using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Squarely.Presentation
{
    /// <summary>
    /// The main menu. Reports the player's choices; whoever owns it decides what happens next.
    /// </summary>
    public sealed class MenuScreen : MonoBehaviour
    {
        [SerializeField] private TMP_Text _titleLabel;
        [SerializeField] private Button _playButton;
        [SerializeField] private TMP_Text _playLabel;
        [SerializeField] private TMP_Text _highScoreLabel;
        [SerializeField] private Button _settingsButton;
        [SerializeField] private TMP_Text _settingsLabel;

        [Tooltip("Opens the test screen. Shown unless the menu's owner hides it.")]
        [SerializeField] private Button _testButton;
        [SerializeField] private TMP_Text _testLabel;

        private MenuTexts _texts;

        /// <summary>The player asked to start a game.</summary>
        public event Action PlayRequestEvent;

        /// <summary>The player asked for the settings panel.</summary>
        public event Action SettingsRequestEvent;

        /// <summary>The player asked for the test screen.</summary>
        public event Action TestRequestEvent;

        #region Unity

        private void Awake()
        {
            _playButton.onClick.AddListener(() => PlayRequestEvent?.Invoke());
            _settingsButton.onClick.AddListener(() => SettingsRequestEvent?.Invoke());
            _testButton.onClick.AddListener(() => TestRequestEvent?.Invoke());
        }

        #endregion

        public void Initialize(MenuTexts texts)
        {
            _texts = texts;
            _titleLabel.text = texts.Title;
            _playLabel.text = texts.Play;
            _settingsLabel.text = texts.Settings;
            _testLabel.text = texts.Test;
        }

        /// <summary>Sets the high score shown. Null, before the first finished game, shows none.</summary>
        public void SetHighScore(int? highScore)
        {
            _highScoreLabel.gameObject.SetActive(highScore.HasValue);

            if (highScore.HasValue)
                _highScoreLabel.text = string.Format(_texts.HighScoreFormat, highScore.Value);
        }

        public void ShowTestButton()
        {
            _testButton.gameObject.SetActive(true);
        }

        public void HideTestButton()
        {
            _testButton.gameObject.SetActive(false);
        }
    }
}
