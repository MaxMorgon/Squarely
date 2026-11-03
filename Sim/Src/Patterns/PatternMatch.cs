using System.Collections.Generic;

namespace Squarely.Simulation
{
    /// <summary>
    /// A pattern found on the board at a specific origin, with the board positions it covers.
    /// </summary>
    public sealed class PatternMatch
    {
        public PatternMatch(BoardPattern pattern, GridPos origin, IReadOnlyList<GridPos> positions)
        {
            Pattern = pattern;
            Origin = origin;
            Positions = positions;
        }

        public BoardPattern Pattern { get; }

        public GridPos Origin { get; }

        public IReadOnlyList<GridPos> Positions { get; }

        public override string ToString()
        {
            return $"{Pattern.Id} at {Origin}";
        }
    }
}
