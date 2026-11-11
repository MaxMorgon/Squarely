using System;
using UnityEngine;

namespace Squarely.Presentation
{
    /// <summary>The game-over screen's texts.</summary>
    [Serializable]
    public sealed class GameOverTexts
    {
        public string WinTitle = "You win!";
        public string LoseTitle = "Game over";

        [Tooltip("{0} is the final score.")]
        public string ScoreFormat = "Score: {0}";

        public string Restart = "Restart";
        public string Menu = "Menu";
    }
}
