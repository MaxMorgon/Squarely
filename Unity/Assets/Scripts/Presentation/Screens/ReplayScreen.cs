using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Squarely.Presentation
{
    /// <summary>
    /// The replay watch screen, shown over the game view: the move counter, a Play/Pause button, Restart,
    /// Step, a speed dropdown, Back, a warning banner and the status or result text.
    /// Reports the player's choices; whoever owns it decides what happens next.
    /// </summary>
    public sealed class ReplayScreen : MonoBehaviour
    {
        [Tooltip("Speed choices and auto-play pace.")]
        [SerializeField] private ReplayFeel _feel;

        [SerializeField] private TMP_Text _progressLabel;

        [SerializeField] private Button _playPauseButton;
        [SerializeField] private TMP_Text _playPauseLabel;
        [SerializeField] private Button _stepButton;
        [SerializeField] private TMP_Text _stepLabel;
        [SerializeField] private Button _restartButton;
        [SerializeField] private TMP_Text _restartLabel;
        [SerializeField] private TMP_Dropdown _speedDropdown;
        [SerializeField] private Button _backButton;
        [SerializeField] private TMP_Text _backLabel;

        [Tooltip("The config warning. Hidden unless the owner shows it.")]
        [SerializeField] private TMP_Text _warningLabel;

        [Tooltip("Shows the divergence message or the result. Hidden while it has no text.")]
        [SerializeField] private TMP_Text _statusLabel;

        private ReplayTexts _texts;

        /// <summary>Seconds auto-play waits between a move's playback and the next move, at speed 1.</summary>
        public float StepDelay => _feel.StepDelay;

        /// <summary>The player pressed Play or Pause.</summary>
        public event Action PlayPauseRequestEvent;

        /// <summary>The player asked to run the next command.</summary>
        public event Action StepRequestEvent;

        /// <summary>The player asked to watch the replay again from the start.</summary>
        public event Action RestartRequestEvent;

        /// <summary>The player picked a playback speed. Gives the speed.</summary>
        public event Action<float> SpeedSelectEvent;

        /// <summary>The player asked to go back.</summary>
        public event Action BackRequestEvent;

        /// <summary>Every frame. Gives the frame time in seconds. Drives the replay's auto-play.</summary>
        public event Action<float> TickEvent;

        #region Unity

        private void Awake()
        {
            _playPauseButton.onClick.AddListener(() => PlayPauseRequestEvent?.Invoke());
            _stepButton.onClick.AddListener(() => StepRequestEvent?.Invoke());
            _restartButton.onClick.AddListener(() => RestartRequestEvent?.Invoke());
            _speedDropdown.onValueChanged.AddListener(index => SpeedSelectEvent?.Invoke(_feel.SpeedPresets[index]));
            _backButton.onClick.AddListener(() => BackRequestEvent?.Invoke());
        }

        private void Update()
        {
            TickEvent?.Invoke(Time.deltaTime);
        }

        #endregion

        public void Initialize(ReplayTexts texts)
        {
            _texts = texts;
            _stepLabel.text = texts.Step;
            _backLabel.text = texts.Back;
            _restartLabel.text = texts.Restart;
            SetSpeedOptions();
            SetPlaying(false);
            SetWarning(false);
            SetStatus(null);
        }

        /// <summary>Sets the move counter: moves shown out of moves in the replay.</summary>
        public void SetProgress(int shown, int total)
        {
            _progressLabel.text = string.Format(_texts.ProgressFormat, shown, total);
        }

        /// <summary>Shows the game-ended text instead of the move counter.</summary>
        public void SetGameEnded()
        {
            _progressLabel.text = _texts.GameEnded;
        }

        /// <summary>Sets whether auto-play runs. The button shows the action it does next: Pause while playing, Play while paused.</summary>
        public void SetPlaying(bool isPlaying)
        {
            _playPauseLabel.text = isPlaying ? _texts.Pause : _texts.Play;
        }

        /// <summary>Selects the speed preset nearest to the speed. Raises no event.</summary>
        public void SetSpeed(float speed)
        {
            int nearest = 0;

            for (int i = 1; i < _feel.SpeedPresets.Length; i++)
            {
                if (Mathf.Abs(_feel.SpeedPresets[i] - speed) < Mathf.Abs(_feel.SpeedPresets[nearest] - speed))
                    nearest = i;
            }

            _speedDropdown.SetValueWithoutNotify(nearest);
        }

        private void SetSpeedOptions()
        {
            List<string> options = new List<string>();

            foreach (float speed in _feel.SpeedPresets)
                options.Add(string.Format(_texts.SpeedFormat, speed));

            _speedDropdown.ClearOptions();
            _speedDropdown.AddOptions(options);
        }

        /// <summary>Shows or hides the banner that says the replay was recorded with a different game config.</summary>
        public void SetWarning(bool isShown)
        {
            _warningLabel.text = _texts.ConfigWarning;
            _warningLabel.gameObject.SetActive(isShown);
        }

        /// <summary>Sets the status or result text. Null or empty hides it.</summary>
        public void SetStatus(string text)
        {
            _statusLabel.text = text;
            _statusLabel.gameObject.SetActive(!string.IsNullOrEmpty(text));
        }

        /// <summary>Sets whether Play/Pause and Step can be pressed. They cannot once the replay is over.</summary>
        public void SetControlsEnabled(bool isEnabled)
        {
            _playPauseButton.interactable = isEnabled;
            _stepButton.interactable = isEnabled;
        }
    }
}
