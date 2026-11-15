using System;
using UnityEngine;

namespace Squarely.Presentation
{
    /// <summary>The test screen's texts.</summary>
    [Serializable]
    public sealed class TestTexts
    {
        public string Title = "Dev Play";
        public string Seed = "Seed";
        public string SeedPlaceholder = "Enter a seed";
        public string Randomize = "Randomize";
        public string HighScore = "High score";
        public string HighScorePlaceholder = "Enter a high score";
        public string StartInTest = "Start in Dev Play";
        public string Play = "Play";
        public string Back = "Back";

        public string ReplaysTitle = "Replays";
        public string HistoryEmpty = "No games yet";

        [Tooltip("{0} is the local date and time, {1} the score, {2} the number of moves, {3} the finished or unfinished text.")]
        public string ReplayRowFormat = "{0:yyyy-MM-dd HH:mm}   Score {1}   Moves {2}   {3}";

        public string ReplayFinished = "Finished";
        public string ReplayUnfinished = "Left early";
        public string ReplayLoad = "Load";
        public string ReplayCopy = "Copy";
        public string ReplayCopied = "Copied";
        public string PastePlaceholder = "Paste a replay code";

        [Tooltip("{0} is the reason the code cannot be read.")]
        public string PasteInvalidFormat = "This replay code is not valid: {0}";

        public string ConfigWarning = "Replay was recorded with a different game config. It may not play the same.";
    }
}
