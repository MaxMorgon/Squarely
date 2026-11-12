using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Squarely.Presentation
{
    /// <summary>
    /// The test screen: a seed field, a high score field, the "start in test screen" toggle,
    /// and the buttons to play, pick a new seed or go back. It also has a replays section:
    /// the list of the last games with Play and Copy buttons, and a field to paste a replay code and play it.
    /// Reports the player's edits and choices; whoever owns it reads the fields and decides what happens next.
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

        [SerializeField] private TMP_Text _highScoreLabel;
        [SerializeField] private TMP_InputField _highScoreField;

        [Tooltip("Shown in the high score field while it is empty.")]
        [SerializeField] private TMP_Text _highScorePlaceholder;

        [SerializeField] private Toggle _startInTestToggle;
        [SerializeField] private TMP_Text _startInTestLabel;

        [SerializeField] private Button _playButton;
        [SerializeField] private TMP_Text _playLabel;
        [SerializeField] private Button _backButton;
        [SerializeField] private TMP_Text _backLabel;

        [Header("Replays")]
        [SerializeField] private TMP_Text _replaysTitleLabel;

        [Tooltip("Parent of the history rows. Give it a layout group.")]
        [SerializeField] private Transform _historyRoot;

        [SerializeField] private ReplayRow _rowPrefab;

        [Tooltip("Shown while the history has no rows.")]
        [SerializeField] private TMP_Text _historyEmptyLabel;

        [SerializeField] private TMP_InputField _replayCodeField;

        [Tooltip("Shown in the replay code field while it is empty.")]
        [SerializeField] private TMP_Text _replayCodePlaceholder;

        [SerializeField] private Button _replayCodePlayButton;
        [SerializeField] private TMP_Text _replayCodePlayLabel;

        [Tooltip("Shows a warning or an error about the replays. Hidden while it has no text.")]
        [SerializeField] private TMP_Text _replayMessageLabel;

        private readonly List<ReplayRow> _rows = new List<ReplayRow>();
        private TestTexts _texts;

        /// <summary>The player changed the seed field's text. Not raised by <see cref="SetSeed"/>.</summary>
        public event Action<string> SeedEditEvent;

        /// <summary>The player changed the high score field's text. Not raised by <see cref="SetHighScore"/>.</summary>
        public event Action<string> HighScoreEditEvent;

        /// <summary>The player switched "start in test screen" on or off. Not raised by <see cref="SetStartInTest"/>.</summary>
        public event Action<bool> StartInTestChangeEvent;

        /// <summary>The player asked for a new random seed.</summary>
        public event Action RandomizeRequestEvent;

        /// <summary>The player asked to play the seed in the field.</summary>
        public event Action PlayRequestEvent;

        /// <summary>The player asked to go back to the menu.</summary>
        public event Action BackRequestEvent;

        /// <summary>The player changed the replay code field's text.</summary>
        public event Action<string> ReplayCodeEditEvent;

        /// <summary>The player asked to play the code in the replay code field.</summary>
        public event Action ReplayCodePlayRequestEvent;

        /// <summary>The player asked to play a history row. Gives the row's index in the list given to <see cref="SetHistory"/>.</summary>
        public event Action<int> HistoryPlayRequestEvent;

        /// <summary>The player asked to copy the code of a history row. Gives the row's index.</summary>
        public event Action<int> HistoryCopyRequestEvent;

        #region Unity

        private void Awake()
        {
            _seedField.onValueChanged.AddListener(text => SeedEditEvent?.Invoke(text));
            _highScoreField.onValueChanged.AddListener(text => HighScoreEditEvent?.Invoke(text));
            _startInTestToggle.onValueChanged.AddListener(isOn => StartInTestChangeEvent?.Invoke(isOn));
            _randomizeButton.onClick.AddListener(() => RandomizeRequestEvent?.Invoke());
            _playButton.onClick.AddListener(() => PlayRequestEvent?.Invoke());
            _backButton.onClick.AddListener(() => BackRequestEvent?.Invoke());
            _replayCodeField.onValueChanged.AddListener(text => ReplayCodeEditEvent?.Invoke(text));
            _replayCodePlayButton.onClick.AddListener(() => ReplayCodePlayRequestEvent?.Invoke());
            _replayMessageLabel.gameObject.SetActive(false);
        }

        #endregion

        public void Initialize(TestTexts texts)
        {
            _texts = texts;
            _titleLabel.text = texts.Title;
            _seedLabel.text = texts.Seed;
            _seedPlaceholder.text = texts.SeedPlaceholder;
            _randomizeLabel.text = texts.Randomize;
            _highScoreLabel.text = texts.HighScore;
            _highScorePlaceholder.text = texts.HighScorePlaceholder;
            _startInTestLabel.text = texts.StartInTest;
            _playLabel.text = texts.Play;
            _backLabel.text = texts.Back;
            _replaysTitleLabel.text = texts.ReplaysTitle;
            _historyEmptyLabel.text = texts.HistoryEmpty;
            _replayCodePlaceholder.text = texts.PastePlaceholder;
            _replayCodePlayLabel.text = texts.ReplayPlay;
        }

        /// <summary>Sets the seed field's text.</summary>
        public void SetSeed(string seedText)
        {
            _seedField.SetTextWithoutNotify(seedText);
        }

        /// <summary>Sets the high score field's text.</summary>
        public void SetHighScore(string highScoreText)
        {
            _highScoreField.SetTextWithoutNotify(highScoreText);
        }

        /// <summary>Sets whether the "start in test screen" toggle is on.</summary>
        public void SetStartInTest(bool isOn)
        {
            _startInTestToggle.SetIsOnWithoutNotify(isOn);
        }

        /// <summary>Sets whether Play can be pressed.</summary>
        public void SetPlayable(bool isPlayable)
        {
            _playButton.interactable = isPlayable;
        }

        #region Replays

        /// <summary>Replaces the history list with one row per text, in order. An empty list shows the "no games" text.</summary>
        public void SetHistory(IReadOnlyList<string> rowTexts)
        {
            foreach (ReplayRow row in _rows)
                Destroy(row.gameObject);

            _rows.Clear();
            _historyEmptyLabel.gameObject.SetActive(rowTexts.Count == 0);

            for (int i = 0; i < rowTexts.Count; i++)
            {
                int index = i;
                ReplayRow row = Instantiate(_rowPrefab, _historyRoot);
                row.SetContent(rowTexts[i], _texts.ReplayPlay, _texts.ReplayCopy);
                row.PlayRequestEvent += () => HistoryPlayRequestEvent?.Invoke(index);
                row.CopyRequestEvent += () => HistoryCopyRequestEvent?.Invoke(index);
                _rows.Add(row);
            }
        }

        /// <summary>Sets whether the replay code field's Play can be pressed.</summary>
        public void SetReplayCodePlayable(bool isPlayable)
        {
            _replayCodePlayButton.interactable = isPlayable;
        }

        /// <summary>Shows a warning or an error about the replays. Null or empty hides it.</summary>
        public void SetReplayMessage(string message)
        {
            _replayMessageLabel.text = message;
            _replayMessageLabel.gameObject.SetActive(!string.IsNullOrEmpty(message));
        }

        #endregion
    }
}
