using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Squarely.Presentation
{
    /// <summary>
    /// Asks the player to confirm an action, with a title, an optional message, and Yes and No.
    /// It blocks input to what is under it. Reports the answer; whoever owns it does the action or not.
    /// </summary>
    public sealed class ConfirmDialog : MonoBehaviour
    {
        [SerializeField] private TMP_Text _titleLabel;
        [SerializeField] private TMP_Text _messageLabel;
        [SerializeField] private Button _yesButton;
        [SerializeField] private TMP_Text _yesLabel;
        [SerializeField] private Button _noButton;
        [SerializeField] private TMP_Text _noLabel;

        /// <summary>The player answered Yes.</summary>
        public event Action ConfirmRequestEvent;

        /// <summary>The player answered No.</summary>
        public event Action CancelRequestEvent;

        #region Unity

        private void Awake()
        {
            _yesButton.onClick.AddListener(() => ConfirmRequestEvent?.Invoke());
            _noButton.onClick.AddListener(() => CancelRequestEvent?.Invoke());
        }

        #endregion

        /// <param name="message">The smaller text under the title. Null or empty hides it.</param>
        public void Initialize(ConfirmTexts texts, string message)
        {
            _titleLabel.text = texts.Title;
            _yesLabel.text = texts.Yes;
            _noLabel.text = texts.No;

            bool hasMessage = !string.IsNullOrEmpty(message);
            _messageLabel.text = hasMessage ? message : string.Empty;
            _messageLabel.gameObject.SetActive(hasMessage);
        }
    }
}
