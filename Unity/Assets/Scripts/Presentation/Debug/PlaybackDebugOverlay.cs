using TMPro;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System.Collections.Generic;
using UnityEngine.InputSystem;
#endif

namespace Squarely.Presentation
{
    /// <summary>
    /// Editor and development builds only: a small panel for tuning playback. Replays the last move (also with R), ends the game at once (also with E),
    /// copies the replay code of the game so far (also with C),
    /// restarts the game, sets the playback speed (back to 1x each play session) and shows the last drift check.
    /// It starts collapsed in every game. A small toggle button (or the backquote key) expands it.
    /// Not game UI. In other builds the component is never created and does nothing.
    /// </summary>
    public sealed class PlaybackDebugOverlay : MonoBehaviour
    {
        [Tooltip("The playback speed presets, one button each.")]
        [SerializeField] private float[] _speeds = { 0.1f, 0.25f, 0.5f, 1f, 2f };

        [SerializeField] private Color _speedColor = new Color(0.25f, 0.25f, 0.25f, 0.9f);
        [SerializeField] private Color _selectedSpeedColor = new Color(0.95f, 0.65f, 0.1f, 1f);

        [Tooltip("Always visible: collapses and expands the panel.")]
        [SerializeField] private Button _toggleButton;
        [SerializeField] private TMP_Text _toggleLabel;

        [Tooltip("Everything that collapses.")]
        [SerializeField] private GameObject _panel;

        [SerializeField] private Button _replayButton;
        [SerializeField] private Button _restartButton;
        [SerializeField] private Button _endGameButton;
        [SerializeField] private Button _copyReplayButton;

        [Tooltip("Inactive by default. Shows briefly after a copy. Put it outside the collapsing panel, so the C key shows it too.")]
        [SerializeField] private TMP_Text _copiedLabel;

        [Tooltip("Inactive template, copied once per speed preset into its parent.")]
        [SerializeField] private Button _speedButtonTemplate;

        [SerializeField] private TMP_Text _driftLabel;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private const string ExpandedToggleText = "×";
        private const string CollapsedToggleText = "DBG";
        private const string CopiedText = "Copied";
        private const float CopiedSeconds = 1.5f;

        // Static, so the chosen speed carries over to the next game of the same play session.
        private static float s_speed = 1f;
        private bool _isExpanded;
        private float _hideCopiedTime;

        private readonly List<Button> _speedButtons = new List<Button>();

        private GameDebugTools _debug;
        private EventPlayer _player;

        public void Initialize(GameDebugTools debug, EventPlayer player)
        {
            _debug = debug;
            _player = player;
            _player.Speed = s_speed;

            _toggleButton.onClick.AddListener(ToggleExpanded);
            _replayButton.onClick.AddListener(_debug.ReplayLastMove);
            _restartButton.onClick.AddListener(_debug.RequestRestart);
            _endGameButton.onClick.AddListener(_debug.EndGame);
            _copyReplayButton.onClick.AddListener(CopyReplay);
            _copiedLabel.gameObject.SetActive(false);
            CreateSpeedButtons();

            SetExpanded(false);
            SetSpeed(s_speed);
            Refresh();
        }

        #region Unity

        // Statics survive between play sessions when domain reload is off: each session starts at normal speed.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetSpeed()
        {
            s_speed = 1f;
        }

        private void Update()
        {
            if (_debug == null)
                return;

            Keyboard keyboard = Keyboard.current;

            if (keyboard != null && keyboard.rKey.wasPressedThisFrame)
                _debug.ReplayLastMove();

            if (keyboard != null && keyboard.eKey.wasPressedThisFrame)
                _debug.EndGame();

            if (keyboard != null && keyboard.cKey.wasPressedThisFrame)
                CopyReplay();

            if (keyboard != null && keyboard.backquoteKey.wasPressedThisFrame)
                ToggleExpanded();

            if (_copiedLabel.gameObject.activeSelf && Time.unscaledTime >= _hideCopiedTime)
                _copiedLabel.gameObject.SetActive(false);

            Refresh();
        }

        #endregion

        private void CopyReplay()
        {
            _debug.CopyReplay();
            _copiedLabel.text = CopiedText;
            _copiedLabel.gameObject.SetActive(true);
            _hideCopiedTime = Time.unscaledTime + CopiedSeconds;
        }

        private void CreateSpeedButtons()
        {
            _speedButtonTemplate.gameObject.SetActive(false);

            foreach (float speed in _speeds)
            {
                Button button = Instantiate(_speedButtonTemplate, _speedButtonTemplate.transform.parent);
                button.name = $"Speed {speed:0.##}x";
                button.GetComponentInChildren<TMP_Text>().text = $"{speed:0.##}x";
                button.onClick.AddListener(() => SetSpeed(speed));
                button.gameObject.SetActive(true);
                _speedButtons.Add(button);
            }
        }

        private void ToggleExpanded()
        {
            SetExpanded(!_isExpanded);
        }

        private void SetExpanded(bool isExpanded)
        {
            _isExpanded = isExpanded;
            _panel.SetActive(isExpanded);
            _toggleLabel.text = isExpanded ? ExpandedToggleText : CollapsedToggleText;
        }

        private void SetSpeed(float speed)
        {
            s_speed = speed;
            _player.Speed = speed;

            for (int i = 0; i < _speedButtons.Count; i++)
            {
                bool isSelected = Mathf.Approximately(_speeds[i], speed);
                _speedButtons[i].targetGraphic.color = isSelected ? _selectedSpeedColor : _speedColor;
                _speedButtons[i].GetComponentInChildren<TMP_Text>().fontStyle = isSelected ? FontStyles.Bold : FontStyles.Normal;
            }
        }

        /// <summary>Sets what changes without the overlay's input: whether replay is possible, and the drift result.</summary>
        private void Refresh()
        {
            _replayButton.interactable = _debug.CanReplayLastMove;
            _endGameButton.interactable = _debug.CanEndGame;

            string driftText = DriftText();

            if (_driftLabel.text != driftText)
                _driftLabel.text = driftText;
        }

        private string DriftText()
        {
            ViewDrift drift = _debug.LastDrift;

            if (drift == null)
                return "Drift check: not run yet";

            if (drift.IsEmpty)
                return "Drift check: passed";

            return "<color=red>Drift check: FAILED, see console</color>";
        }
#endif
    }
}
