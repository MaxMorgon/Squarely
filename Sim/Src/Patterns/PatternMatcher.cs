using System.Collections.Generic;

namespace Squarely.Simulation
{
    /// <summary>
    /// Finds every match of the given patterns on a board.
    /// Order is deterministic: patterns in list order; for unanchored patterns, origins bottom row first, left to right.
    /// </summary>
    public static class PatternMatcher
    {
        public static List<PatternMatch> FindMatches(IReadOnlyBoard board, IReadOnlyList<BoardPattern> patterns)
        {
            var matches = new List<PatternMatch>();

            foreach (BoardPattern pattern in patterns)
            {
                if (pattern.Anchor.HasValue)
                {
                    TryAddMatch(board, pattern, pattern.Anchor.Value, matches);
                    continue;
                }

                for (int y = 0; y <= board.Height - pattern.Height; y++)
                {
                    for (int x = 0; x <= board.Width - pattern.Width; x++)
                        TryAddMatch(board, pattern, new GridPos(x, y), matches);
                }
            }

            return matches;
        }

        public static bool IsMatchAt(IReadOnlyBoard board, BoardPattern pattern, GridPos origin)
        {
            foreach (PatternCell cell in pattern.Cells)
            {
                GridPos pos = origin + cell.Offset;

                if (!board.Contains(pos) || !cell.Condition.Matches(board.GetCell(pos)))
                    return false;
            }

            return true;
        }

        private static void TryAddMatch(IReadOnlyBoard board, BoardPattern pattern, GridPos origin, List<PatternMatch> matches)
        {
            if (!IsMatchAt(board, pattern, origin))
                return;

            var positions = new GridPos[pattern.Cells.Count];

            for (int i = 0; i < positions.Length; i++)
                positions[i] = origin + pattern.Cells[i].Offset;

            matches.Add(new PatternMatch(pattern, origin, positions));
        }
    }
}
