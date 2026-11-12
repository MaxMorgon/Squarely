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
    }
}
