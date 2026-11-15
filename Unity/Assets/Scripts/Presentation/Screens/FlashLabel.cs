using System;
using TMPro;

namespace Squarely.Presentation
{
    /// <summary>
    /// A text label that shows a short message ("Copied") and hides itself after a time.
    /// The owner calls <see cref="Tick"/> every frame. The label starts hidden.
    /// </summary>
    public sealed class FlashLabel
    {
        private const float DefaultSeconds = 1.5f;

        private readonly Action<string> _setText;
        private readonly Action<bool> _setVisible;
        private readonly float _seconds;

        private float _hideTime;

        /// <summary>Whether the message is showing.</summary>
        public bool IsVisible { get; private set; }

        /// <param name="label">The label to show the message in. Its object is switched on and off.</param>
        /// <param name="seconds">How long a message shows. The default is 1.5 seconds.</param>
        public FlashLabel(TMP_Text label, float seconds = DefaultSeconds)
            : this(text => label.text = text, visible => label.gameObject.SetActive(visible), seconds)
        {
        }

        internal FlashLabel(Action<string> setText, Action<bool> setVisible, float seconds)
        {
            _setText = setText;
            _setVisible = setVisible;
            _seconds = seconds;
            Hide();
        }

        /// <summary>Shows the message at once. It hides <c>seconds</c> after <paramref name="now"/>. A new message restarts the time.</summary>
        public void Flash(string text, float now)
        {
            _setText(text);
            _setVisible(true);
            IsVisible = true;
            _hideTime = now + _seconds;
        }

        /// <summary>Hides the message at once.</summary>
        public void Hide()
        {
            _setVisible(false);
            IsVisible = false;
        }

        /// <summary>Hides the message once its time is up.</summary>
        public void Tick(float now)
        {
            if (IsVisible && now >= _hideTime)
                Hide();
        }
    }
}
