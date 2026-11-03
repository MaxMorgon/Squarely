using System;
using System.Collections.Generic;

namespace Squarely.Simulation
{
    /// <summary>
    /// An immutable piece shape: a set of cell offsets, normalized so the lowest X and Y are 0.
    /// </summary>
    public sealed class PieceShape
    {
        private readonly GridPos[] _cells;

        public PieceShape(string id, IEnumerable<GridPos> cells)
        {
            if (string.IsNullOrEmpty(id))
                throw new ArgumentException("Piece id is required.", nameof(id));

            if (cells == null)
                throw new ArgumentNullException(nameof(cells));

            Id = id;
            _cells = Normalize(cells);

            if (_cells.Length == 0)
                throw new ArgumentException($"Piece '{id}' has no cells.", nameof(cells));

            foreach (GridPos cell in _cells)
            {
                Width = Math.Max(Width, cell.X + 1);
                Height = Math.Max(Height, cell.Y + 1);
            }
        }

        public string Id { get; }

        public IReadOnlyList<GridPos> Cells => _cells;

        public int CellCount => _cells.Length;

        public int Width { get; }

        public int Height { get; }

        #region Pattern parsing

        /// <summary>
        /// Builds a shape from a text pattern. Rows are separated by '/', written top row first.
        /// '#' is a filled cell, '.' is empty. Example: an L shape is "#./#./##".
        /// </summary>
        public static PieceShape FromPattern(string id, string pattern)
        {
            if (string.IsNullOrEmpty(pattern))
                throw new ArgumentException($"Piece '{id}' has an empty pattern.", nameof(pattern));

            string[] rows = pattern.Split('/');
            var cells = new List<GridPos>();

            for (int rowIndex = 0; rowIndex < rows.Length; rowIndex++)
            {
                // Pattern rows are written top-down; board Y grows upward.
                int y = rows.Length - 1 - rowIndex;
                string row = rows[rowIndex];

                for (int x = 0; x < row.Length; x++)
                {
                    char symbol = row[x];

                    if (symbol == '#')
                        cells.Add(new GridPos(x, y));
                    else if (symbol != '.')
                        throw new FormatException($"Piece '{id}' has invalid character '{symbol}' in pattern '{pattern}'.");
                }
            }

            return new PieceShape(id, cells);
        }

        #endregion

        private static GridPos[] Normalize(IEnumerable<GridPos> cells)
        {
            var unique = new List<GridPos>();

            foreach (GridPos cell in cells)
            {
                if (!unique.Contains(cell))
                    unique.Add(cell);
            }

            if (unique.Count == 0)
                return Array.Empty<GridPos>();

            int minX = int.MaxValue;
            int minY = int.MaxValue;

            foreach (GridPos cell in unique)
            {
                minX = Math.Min(minX, cell.X);
                minY = Math.Min(minY, cell.Y);
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
