using System;

namespace Squarely.App
{
    /// <summary>One saved game in the <see cref="ReplayHistory"/>.</summary>
    public sealed class ReplayHistoryEntry
    {
        /// <summary>The replay code of the game (see ReplayCodec).</summary>
        public string Code { get; }

        /// <summary>When the entry was saved, in UTC.</summary>
        public DateTime SavedAtUtc { get; }

        public int Score { get; }

        /// <summary>How many pieces the player placed.</summary>
        public int MoveCount { get; }

        /// <summary>Whether the game ended (true) or the player left it (false).</summary>
        public bool IsFinished { get; }

        public ReplayHistoryEntry(string code, DateTime savedAtUtc, int score, int moveCount, bool isFinished)
        {
            Code = code;
            SavedAtUtc = savedAtUtc;
            Score = score;
            MoveCount = moveCount;
            IsFinished = isFinished;
        }
    }
}
