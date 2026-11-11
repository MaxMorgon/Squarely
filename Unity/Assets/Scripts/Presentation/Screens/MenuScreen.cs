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

        private MenuTexts _texts;

        /// <summary>The player asked to start a game.</summary>
        public event Action PlayRequestEvent;

        #region Unity

        private void Awake()
        {
            _playButton.onClick.AddListener(() => PlayRequestEvent?.Invoke());
        }

        #endregion

        public void Initialize(MenuTexts texts)
        {
            _texts = texts;
            _titleLabel.text = texts.Title;
            _playLabel.text = texts.Play;
        }

        /// <summary>Sets the high score shown. Null, before the first finished game, shows none.</summary>
        public void SetHighScore(int? highScore)
        {
            _highScoreLabel.gameObject.SetActive(highScore.HasValue);

            if (highScore.HasValue)
                _highScoreLabel.text = string.Format(_texts.HighScoreFormat, highScore.Value);
        }
    }
}
