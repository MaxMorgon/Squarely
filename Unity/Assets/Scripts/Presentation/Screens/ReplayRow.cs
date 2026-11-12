using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Squarely.Presentation
{
    /// <summary>One row of the test screen's replay list: a description with Load and Copy buttons.</summary>
    public sealed class ReplayRow : MonoBehaviour
    {
        [SerializeField] private TMP_Text _label;
        [SerializeField] private Button _playButton;
        [SerializeField] private TMP_Text _playLabel;
        [SerializeField] private Button _copyButton;
        [SerializeField] private TMP_Text _copyLabel;

        /// <summary>The player pressed Play on this row.</summary>
        public event Action PlayRequestEvent;

        /// <summary>The player pressed Copy on this row.</summary>
        public event Action CopyRequestEvent;

        #region Unity

        private void Awake()
        {
            _playButton.onClick.AddListener(() => PlayRequestEvent?.Invoke());
            _copyButton.onClick.AddListener(() => CopyRequestEvent?.Invoke());
        }

        #endregion

        public void SetContent(string text, string playText, string copyText)
        {
            _label.text = text;
            _playLabel.text = playText;
            _copyLabel.text = copyText;
        }
    }
}
