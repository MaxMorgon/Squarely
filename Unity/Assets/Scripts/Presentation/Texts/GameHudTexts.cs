using System;
using UnityEngine;

namespace Squarely.Presentation
{
    /// <summary>The in-game HUD's texts.</summary>
    [Serializable]
    public sealed class GameHudTexts
    {
        [Tooltip("{0} is the combo count.")]
        public string ComboFormat = "Combo ×{0}";

        [Tooltip("Shown once in a game, when the score goes above the best score.")]
        public string NewRecord = "New record!";

        public string Settings = "Settings";
    }
}
