using System;
using UnityEngine;

namespace Squarely.Presentation
{
    /// <summary>The main menu's texts.</summary>
    [Serializable]
    public sealed class MenuTexts
    {
        public string Title = "Squarely";
        public string Play = "Play";
        public string Settings = "Settings";
        public string Test = "Dev Play";

        [Tooltip("{0} is the high score.")]
        public string HighScoreFormat = "Best: {0}";
    }
}
