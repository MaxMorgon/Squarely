using System;
using System.Collections.Generic;

namespace Squarely.Simulation
{
    /// <summary>
    /// An immutable piece definition: cell offsets (normalized so the lowest X and Y are 0)
    /// and the cell type its blocks become when placed.
    /// </summary>
    public sealed class PieceShape
    {
        private readonly GridPos[] _offsets;

        public PieceShape(string id, IEnumerable<GridPos> offsets, string cellTypeId = Cell.BlockType)
        {
            if (string.IsNullOrEmpty(id))
                throw new ArgumentException("Piece id is required.", nameof(id));

            if (offsets == null)
                throw new ArgumentNullException(nameof(offsets));

            if (string.IsNullOrEmpty(cellTypeId))
                throw new ArgumentException($"Piece '{id}' needs a cell type id.", nameof(cellTypeId));

            Id = id;
            CellTypeId = cellTypeId;
            _offsets = Normalize(offsets);

            if (_offsets.Length == 0)
                throw new ArgumentException($"Piece '{id}' has no cells.", nameof(offsets));

            foreach (GridPos offset in _offsets)
            {
                Width = Math.Max(Width, offset.X + 1);
                Height = Math.Max(Height, offset.Y + 1);
            }
        }

        public string Id { get; }

        public string CellTypeId { get; }

        public IReadOnlyList<GridPos> Offsets => _offsets;

        public int CellCount => _offsets.Length;

        public int Width { get; }

        public int Height { get; }

        #region Pattern parsing

        /// <summary>
        /// Builds a shape from a text pattern. Rows are separated by '/', written top row first.
        /// '#' is a filled cell, '.' is empty. Example: an L shape is "#./#./##".
        /// </summary>
        public static PieceShape FromPattern(string id, string pattern, string cellTypeId = Cell.BlockType)
        {
            if (string.IsNullOrEmpty(pattern))
                throw new ArgumentException($"Piece '{id}' has an empty pattern.", nameof(pattern));

            string[] rows = pattern.Split('/');
            var offsets = new List<GridPos>();

            for (int rowIndex = 0; rowIndex < rows.Length; rowIndex++)
            {
                // Pattern rows are written top-down; board Y grows upward.
                int y = rows.Length - 1 - rowIndex;
                string row = rows[rowIndex];

                for (int x = 0; x < row.Length; x++)
                {
                    char symbol = row[x];

                    if (symbol == '#')
                        offsets.Add(new GridPos(x, y));
                    else if (symbol != '.')
                        throw new FormatException($"Piece '{id}' has invalid character '{symbol}' in pattern '{pattern}'.");
                }
            }

            return new PieceShape(id, offsets, cellTypeId);
        }

        #endregion

        private static GridPos[] Normalize(IEnumerable<GridPos> offsets)
        {
            var unique = new List<GridPos>();

            foreach (GridPos offset in offsets)
            {
                if (!unique.Contains(offset))
                    unique.Add(offset);
            }

            if (unique.Count == 0)
                return Array.Empty<GridPos>();

            int minX = int.MaxValue;
            int minY = int.MaxValue;

            foreach (GridPos offset in unique)
            {
                minX = Math.Min(minX, offset.X);
                minY = Math.Min(minY, offset.Y);
            }

            var normalized = new GridPos[unique.Count];

            for (int i = 0; i < unique.Count; i++)
                normalized[i] = new GridPos(unique[i].X - minX, unique[i].Y - minY);

            return normalized;
        }

        public override string ToString()
        {
            return Id;
        }
    }
}
