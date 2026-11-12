using System;

namespace Squarely.Simulation
{
    /// <summary>
    /// Config data for one piece in the deal pool.
    /// Public fields (not properties) so Unity's serializer can edit it.
    /// </summary>
    [Serializable]
    public sealed class PieceDefinition
    {
        public string Id;

        /// <summary>
        /// Rows separated by '/', top row first. '#' = filled, '.' = empty. See <see cref="PieceShape.FromPattern"/>.
        /// </summary>
        public string Pattern;

        /// <summary>Relative chance of being dealt. 0 removes the piece from the pool.</summary>
        public int Weight = 1;

        /// <summary>The cell type the piece's blocks become when placed.</summary>
        public string CellTypeId = Cell.BlockType;

        public PieceDefinition()
        {
        }

        public PieceDefinition(string id, string pattern, int weight = 1)
        {
            Id = id;
            Pattern = pattern;
            Weight = weight;
        }

        /// <summary>An independent copy: edits to either one don't affect the other.</summary>
        internal PieceDefinition Clone()
        {
            return (PieceDefinition)MemberwiseClone();
        }
    }
}
