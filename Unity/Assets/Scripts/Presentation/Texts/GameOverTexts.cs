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

        [Tooltip("{0} is the high score this game did not beat.")]
        public string HighScoreFormat = "Best: {0}";

        public string NewHighScore = "New high score!";

        public string Restart = "Restart";
        public string Menu = "Menu";

        [Tooltip("Editor and dev-tools builds only.")]
        public string CopyReplay = "Copy replay";

        [Tooltip("Feedback after the replay code is copied. Editor and dev-tools builds only.")]
        public string ReplayCopied = "Copied";
    }
}
