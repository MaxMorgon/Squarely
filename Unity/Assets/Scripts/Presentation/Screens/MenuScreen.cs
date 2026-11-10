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
            _titleLabel.text = texts.Title;
            _playLabel.text = texts.Play;
        }
    }
}
