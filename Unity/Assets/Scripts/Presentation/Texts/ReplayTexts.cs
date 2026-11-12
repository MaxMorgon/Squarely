using System;
using UnityEngine;

namespace Squarely.Presentation
{
    /// <summary>The replay watch screen's texts.</summary>
    [Serializable]
    public sealed class ReplayTexts
    {
        public string Play = "Play";
        public string Pause = "Pause";
        public string Step = "Step";
        public string Back = "Back";

        [Tooltip("{0} is the playback speed.")]
        public string SpeedFormat = "x{0}";

        [Tooltip("{0} is the number of moves shown, {1} the number of moves in the replay.")]
        public string ProgressFormat = "Move {0} / {1}";

        public string ConfigWarning = "Replay was recorded with a different game config. It may not play the same.";

        [Tooltip("{0} is the number of the move that failed.")]
        public string DivergedFormat = "Replay diverged at move {0}";

        [Tooltip("{0} is the final score.")]
        public string ResultFormat = "Final score: {0}";

        public string ResultMatch = "Same result as recorded.";

        [Tooltip("{0} is the recorded score, {1} the recorded finished or unfinished text.")]
        public string ResultMismatchFormat = "Different from the recording: score {0}, {1}.";

        public string ResultFinished = "finished";
        public string ResultUnfinished = "not finished";
    }
}
