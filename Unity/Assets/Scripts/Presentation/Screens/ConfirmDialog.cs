using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Squarely.Presentation
{
    /// <summary>
    /// Asks the player to confirm an action, with Yes and No. It blocks input to what is under it.
    /// Reports the answer; whoever owns it does the action or not.
    /// </summary>
    public sealed class ConfirmDialog : MonoBehaviour
    {
        [SerializeField] private TMP_Text _questionLabel;
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

        /// <param name="question">The question to show instead of the default one.</param>
        public void Initialize(ConfirmTexts texts, string question)
        {
            Initialize(texts);
            _questionLabel.text = question;
        }

        public void Initialize(ConfirmTexts texts)
        {
            _questionLabel.text = texts.Question;
            _yesLabel.text = texts.Yes;
            _noLabel.text = texts.No;
        }
    }
}
