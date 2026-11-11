using System;

namespace Squarely.Simulation
{
    /// <summary>
    /// Config data for a custom board pattern (pixel art, small shapes, ...).
    /// </summary>
    [Serializable]
    public sealed class BoardPatternDefinition
    {
        public string Id;

        /// <summary>
        /// Rows separated by '/', top row first. '#' = must be filled, 'o' = must be empty, '.' = not part of the pattern.
        /// See <see cref="BoardPattern.FromText"/>.
        /// </summary>
        public string Pattern;

        /// <summary>Points for completing the pattern.</summary>
        public int Points;

        /// <summary>If true, the pattern only matches with its origin at (AnchorX, AnchorY); otherwise anywhere.</summary>
        public bool IsAnchored;

        public int AnchorX;

        public int AnchorY;

        /// <summary>An independent copy: edits to either one don't affect the other.</summary>
        internal BoardPatternDefinition Clone()
        {
            return (BoardPatternDefinition)MemberwiseClone();
        }

        public BoardPattern ToPattern()
        {
            GridPos? anchor = IsAnchored ? new GridPos(AnchorX, AnchorY) : (GridPos?)null;
            return BoardPattern.FromText(Id, Pattern, Points, anchor);
        }
    }
}
