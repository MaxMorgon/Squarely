using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Squarely.Presentation
{
    /// <summary>
    /// The in-game HUD: the score view, the settings button and the "new record" celebration.
    /// Reports the player's choices; whoever owns the game decides what happens next.
    /// </summary>
    public sealed class GameHud : MonoBehaviour
    {
        [SerializeField] private ScoreView _score;

        [SerializeField] private Button _settingsButton;
        [SerializeField] private TMP_Text _settingsLabel;

        [Tooltip("Shows the \"new record\" text. Hidden until the celebration shows it. Must not block input.")]
        [SerializeField] private TMP_Text _newRecordLabel;

        [Tooltip("Plays when the score goes above the best score. Hang the celebration (animation, particles, sound) here.")]
        [SerializeField] private UnityEvent _newRecordCelebration = new UnityEvent();

        public ScoreView Score => _score;

        /// <summary>The player asked for the settings panel.</summary>
        public event Action SettingsRequestEvent;

        #region Unity

        private void Awake()
        {
            _settingsButton.onClick.AddListener(() => SettingsRequestEvent?.Invoke());
        }

        #endregion

        public void Initialize(GameHudTexts texts)
        {
            _score.Initialize(texts);
            _settingsLabel.text = texts.Settings;
            _newRecordLabel.text = texts.NewRecord;
        }

        /// <summary>Plays the "new record" celebration.</summary>
        public void CelebrateNewRecord()
        {
            _newRecordCelebration.Invoke();
        }
    }
}
