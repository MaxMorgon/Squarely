using System.Collections.Generic;

namespace Squarely.Simulation
{
    /// <summary>
    /// One match that was cleared, with the cells it emptied, so each match can get its own effects (e.g. at its centroid).
    /// </summary>
    public sealed class ClearedMatch
    {
        public PatternMatch Match { get; }

        /// <summary>
        /// The match's cells that were filled before the clear, in board order.
        /// A cell shared with another match is listed in both. Cells the pattern requires empty are not listed.
        /// </summary>
        public IReadOnlyList<GridPos> Cells { get; }

        public ClearedMatch(PatternMatch match, IReadOnlyList<GridPos> cells)
        {
            Match = match;
            Cells = cells;
        }

        public override string ToString()
        {
            return $"{Match} ({Cells.Count} cells)";
        }
    }
}
