using System;
using UnityEngine;
using UnityEngine.UI;

namespace Squarely.Presentation
{
    /// <summary>
    /// The main menu. Reports the player's choices; whoever owns it decides what happens next.
    /// </summary>
    public sealed class MenuScreen : MonoBehaviour
    {
        [SerializeField] private Button _playButton;

        /// <summary>The player asked to start a game.</summary>
        public event Action PlayRequested;

        #region Unity

        private void Awake()
        {
            _playButton.onClick.AddListener(() => PlayRequested?.Invoke());
        }

        #endregion
    }
}
