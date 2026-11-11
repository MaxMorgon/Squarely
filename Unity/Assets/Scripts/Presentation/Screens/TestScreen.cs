using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Squarely.Presentation
{
    /// <summary>
    /// The test screen: a seed field and the buttons to play it, pick a new one or go back.
    /// Reports the player's edits and choices; whoever owns it reads the seed and decides what happens next.
    /// </summary>
    public sealed class TestScreen : MonoBehaviour
    {
        [SerializeField] private TMP_Text _titleLabel;
        [SerializeField] private TMP_Text _seedLabel;
        [SerializeField] private TMP_InputField _seedField;

        [Tooltip("Shown in the seed field while it is empty.")]
        [SerializeField] private TMP_Text _seedPlaceholder;

        [SerializeField] private Button _randomizeButton;
        [SerializeField] private TMP_Text _randomizeLabel;
        [SerializeField] private Button _playButton;
        [SerializeField] private TMP_Text _playLabel;
        [SerializeField] private Button _backButton;
        [SerializeField] private TMP_Text _backLabel;

        /// <summary>The player changed the seed field's text. Not raised by <see cref="SetSeed"/>.</summary>
        public event Action<string> SeedEditEvent;

        /// <summary>The player asked for a new random seed.</summary>
        public event Action RandomizeRequestEvent;

        /// <summary>The player asked to play the seed in the field.</summary>
        public event Action PlayRequestEvent;

        /// <summary>The player asked to go back to the menu.</summary>
        public event Action BackRequestEvent;

        #region Unity

        private void Awake()
        {
            _seedField.onValueChanged.AddListener(text => SeedEditEvent?.Invoke(text));
            _randomizeButton.onClick.AddListener(() => RandomizeRequestEvent?.Invoke());
            _playButton.onClick.AddListener(() => PlayRequestEvent?.Invoke());
            _backButton.onClick.AddListener(() => BackRequestEvent?.Invoke());
        }

        #endregion

        public void Initialize(TestTexts texts)
        {
            _titleLabel.text = texts.Title;
            _seedLabel.text = texts.Seed;
            _seedPlaceholder.text = texts.SeedPlaceholder;
            _randomizeLabel.text = texts.Randomize;
            _playLabel.text = texts.Play;
            _backLabel.text = texts.Back;
        }

        /// <summary>Sets the seed field's text.</summary>
        public void SetSeed(string seedText)
        {
            _seedField.SetTextWithoutNotify(seedText);
        }

        /// <summary>Sets whether Play can be pressed.</summary>
        public void SetPlayable(bool isPlayable)
        {
            _playButton.interactable = isPlayable;
        }
    }
}
