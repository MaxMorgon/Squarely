using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Squarely.Presentation
{
    /// <summary>
    /// The settings panel, shown over the menu or the game. It blocks input to what is under it.
    /// In the game it also has Restart and Quit, and its close button reads Resume.
    /// Reports the player's choices; whoever owns it decides what happens next.
    /// </summary>
    public sealed class SettingsPanel : MonoBehaviour
    {
        [SerializeField] private TMP_Text _titleLabel;

        [Tooltip("Closes the panel: Close in the menu, Resume in the game.")]
        [SerializeField] private Button _closeButton;
        [SerializeField] private TMP_Text _closeLabel;

        [Tooltip("Game only.")]
        [SerializeField] private Button _restartButton;
        [SerializeField] private TMP_Text _restartLabel;

        [Tooltip("Game only.")]
        [SerializeField] private Button _quitButton;
        [SerializeField] private TMP_Text _quitLabel;

        /// <summary>The player asked to close the panel.</summary>
        public event Action CloseRequestEvent;

        /// <summary>The player asked to restart the game. Game only.</summary>
        public event Action RestartRequestEvent;

        /// <summary>The player asked to quit the game to the menu. Game only.</summary>
        public event Action QuitRequestEvent;

        #region Unity

        private void Awake()
        {
            _closeButton.onClick.AddListener(() => CloseRequestEvent?.Invoke());
            _restartButton.onClick.AddListener(() => RestartRequestEvent?.Invoke());
            _quitButton.onClick.AddListener(() => QuitRequestEvent?.Invoke());
        }

        #endregion

        /// <param name="isInGame">True over a game: shows Restart and Quit, and the close button reads Resume.</param>
        public void Initialize(SettingsTexts texts, bool isInGame)
        {
            _titleLabel.text = texts.Title;
            _closeLabel.text = isInGame ? texts.Resume : texts.Close;
            _restartLabel.text = texts.Restart;
            _quitLabel.text = texts.Quit;

            _restartButton.gameObject.SetActive(isInGame);
            _quitButton.gameObject.SetActive(isInGame);
        }
    }
}
