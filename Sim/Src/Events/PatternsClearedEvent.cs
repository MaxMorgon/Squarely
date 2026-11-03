using System.Collections.Generic;

namespace Squarely.Simulation
{
    public sealed class PatternsClearedEvent : ISimEvent
    {
        public PatternsClearedEvent(IReadOnlyList<PatternMatch> matches, IReadOnlyList<GridPos> clearedCells)
        {
            Matches = matches;
            ClearedCells = clearedCells;
        }

        public IReadOnlyList<PatternMatch> Matches { get; }

        /// <summary>Every cell that was emptied, each once, in board order.</summary>
        public IReadOnlyList<GridPos> ClearedCells { get; }
    }
}
