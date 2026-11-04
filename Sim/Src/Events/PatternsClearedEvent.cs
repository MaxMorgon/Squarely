using System.Collections.Generic;

namespace Squarely.Simulation
{
    public sealed class PatternsClearedEvent : ISimEvent
    {
        /// <summary>Every match, in matcher order, with the cells each one emptied.</summary>
        public IReadOnlyList<ClearedMatch> Matches { get; }

        /// <summary>Every cell that was emptied, each once, in board order.</summary>
        public IReadOnlyList<GridPos> ClearedCells { get; }

        public PatternsClearedEvent(IReadOnlyList<ClearedMatch> matches, IReadOnlyList<GridPos> clearedCells)
        {
            Matches = matches;
            ClearedCells = clearedCells;
        }
    }
}
