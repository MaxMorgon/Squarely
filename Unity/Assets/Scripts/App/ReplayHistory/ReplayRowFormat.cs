using System;
using System.Globalization;

namespace Squarely.App
{
    /// <summary>Makes the text of a replay list row.</summary>
    public static class ReplayRowFormat
    {
        /// <param name="localTime">The entry's save time in the local time zone.</param>
        /// <param name="format">{0} is the time, {1} the score, {2} the move count, {3} the finished or unfinished text.</param>
        public static string Format(ReplayHistoryEntry entry, DateTime localTime, string format, string finishedText, string unfinishedText)
        {
            string state = entry.IsFinished ? finishedText : unfinishedText;
            return string.Format(CultureInfo.InvariantCulture, format, localTime, entry.Score, entry.MoveCount, state);
        }
    }
}
