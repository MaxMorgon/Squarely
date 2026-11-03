using System;
using System.Collections.Generic;

namespace Squarely.Simulation
{
    /// <summary>
    /// The play grid. Handles placement and line clearing only; knows nothing about score or hands.
    /// </summary>
    public sealed class Board
    {
        private readonly Cell[] _cells;

        public Board(int width, int height)
        {
            if (width <= 0)
                throw new ArgumentOutOfRangeException(nameof(width), "Board width must be positive.");

            if (height <= 0)
                throw new ArgumentOutOfRangeException(nameof(height), "Board height must be positive.");

            Width = width;
            Height = height;
            _cells = new Cell[width * height];
        }

        public int Width { get; }

        public int Height { get; }

        #region Cell access

        public bool Contains(GridPos pos)
        {
            return pos.X >= 0 && pos.X < Width && pos.Y >= 0 && pos.Y < Height;
        }

        public Cell GetCell(GridPos pos)
        {
            return _cells[IndexOf(pos)];
        }

        public void SetCell(GridPos pos, Cell cell)
        {
            _cells[IndexOf(pos)] = cell;
        }

        public bool IsEmpty(GridPos pos)
        {
            return GetCell(pos).IsEmpty;
        }

        private int IndexOf(GridPos pos)
        {
            if (!Contains(pos))
                throw new ArgumentOutOfRangeException(nameof(pos), $"Position {pos} is outside the {Width}x{Height} board.");

            return pos.Y * Width + pos.X;
        }

        #endregion

        #region Placement

        public bool CanPlace(PieceShape shape, GridPos origin)
        {
            foreach (GridPos offset in shape.Cells)
            {
                GridPos pos = origin + offset;

                if (!Contains(pos) || !IsEmpty(pos))
                    return false;
            }

            return true;
        }

        public bool CanPlaceAnywhere(PieceShape shape)
        {
            for (int y = 0; y <= Height - shape.Height; y++)
            {
                for (int x = 0; x <= Width - shape.Width; x++)
                {
                    if (CanPlace(shape, new GridPos(x, y)))
                        return true;
                }
            }

            return false;
        }

        public void Place(PieceShape shape, GridPos origin, Cell cell)
        {
            if (!CanPlace(shape, origin))
                throw new InvalidOperationException($"Piece '{shape.Id}' cannot be placed at {origin}.");

            foreach (GridPos offset in shape.Cells)
                SetCell(origin + offset, cell);
        }

        #endregion

        #region Line clearing

        public BoardLines FindFullLines()
        {
            var rows = new List<int>();
            var columns = new List<int>();

            for (int y = 0; y < Height; y++)
            {
                if (IsRowFull(y))
                    rows.Add(y);
            }

            for (int x = 0; x < Width; x++)
            {
                if (IsColumnFull(x))
                    columns.Add(x);
            }

            if (rows.Count == 0 && columns.Count == 0)
                return BoardLines.None;

            return new BoardLines(rows, columns);
        }

        /// <summary>
        /// Clears all given rows and columns at once. A cell on both a cleared row and column is cleared once.
        /// Returns the cleared cells in board order (bottom row first, left to right).
        /// </summary>
        public IReadOnlyList<GridPos> ClearLines(BoardLines lines)
        {
            var cleared = new List<GridPos>();

            if (lines.IsEmpty)
                return cleared;

            var clearRow = new bool[Height];
            var clearColumn = new bool[Width];

            foreach (int y in lines.Rows)
                clearRow[y] = true;

            foreach (int x in lines.Columns)
                clearColumn[x] = true;

            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    if (!clearRow[y] && !clearColumn[x])
                        continue;

                    var pos = new GridPos(x, y);

                    if (IsEmpty(pos))
                        continue;

                    SetCell(pos, Cell.Empty);
                    cleared.Add(pos);
                }
            }

            return cleared;
        }

        private bool IsRowFull(int y)
        {
            for (int x = 0; x < Width; x++)
            {
                if (IsEmpty(new GridPos(x, y)))
                    return false;
            }

            return true;
        }

        private bool IsColumnFull(int x)
        {
            for (int y = 0; y < Height; y++)
            {
                if (IsEmpty(new GridPos(x, y)))
                    return false;
            }

            return true;
        }

        #endregion
    }
}
