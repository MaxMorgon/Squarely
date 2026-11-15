using System;

namespace Squarely.Presentation
{
    /// <summary>The settings panel's texts, in the menu and in the game.</summary>
    [Serializable]
    public sealed class SettingsTexts
    {
        public string Title = "Settings";

        /// <summary>Closes the panel in the menu.</summary>
        public string Close = "Close";

        /// <summary>Closes the panel in the game.</summary>
        public string Resume = "Resume";

        public string Restart = "Restart";
        public string Quit = "Quit to menu";

        public string Sfx = "Sound effects";
        public string Music = "Music";

        /// <summary>The dev tools toggle. Shown only in builds with dev tools.</summary>
        public string DevTools = "Dev tools";

        /// <summary>The toggle for the ring that follows the pointer in screen recordings. Shown with the dev tools toggle, while dev tools are on.</summary>
        public string CursorMarker = "Show cursor";

        /// <summary>The reset progress button. Shown in the menu settings, in builds with dev tools, while dev tools are on.</summary>
        public string ResetProgress = "Reset progress";
    }
}
