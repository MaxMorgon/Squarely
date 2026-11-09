using System;
using System.Collections.Generic;

namespace Squarely.Simulation
{
    /// <summary>
    /// The play grid. Handles cell state and placement only; matching lives in <see cref="PatternMatcher"/>.
    /// Mutators are internal so only the sim (and tests) can change the board.
    /// </summary>
    public sealed class Board : IReadOnlyBoard
    {
        private readonly Grid<Cell> _cells;

        public int Width => _cells.Width;

        public int Height => _cells.Height;

        public Board(int width, int height)
            : this(new Grid<Cell>(width, height))
        {
        }

        private Board(Grid<Cell> cells)
        {
            _cells = cells;
        }

        #region Reading

        public bool Contains(GridPos pos)
        {
            return _cells.Contains(pos);
        }

        public Cell GetCell(GridPos pos)
        {
            return _cells[pos];
        }

        public bool IsEmpty(GridPos pos)
        {
            return GetCell(pos).IsEmpty;
        }

        public bool CanPlace(PieceShape shape, GridPos origin)
        {
            foreach (GridPos offset in shape.Offsets)
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

        #endregion

        #region Mutation

        /// <summary>A deep copy, for preview clones of the sim.</summary>
        internal Board Clone()
        {
            return new Board(_cells.Clone());
        }

        internal void SetCell(GridPos pos, Cell cell)
        {
            _cells[pos] = cell;
        }

        internal void Place(PieceShape shape, GridPos origin)
        {
            if (!CanPlace(shape, origin))
                throw new InvalidOperationException($"Piece '{shape.Id}' cannot be placed at {origin}.");

            var cell = new Cell(shape.CellTypeId);

            foreach (GridPos offset in shape.Offsets)
                SetCell(origin + offset, cell);
        }

        /// <summary>
        /// Empties the given positions. Duplicates are cleared once.
        /// Returns the positions that were filled before, in board order (bottom row first, left to right).
        /// </summary>
        internal IReadOnlyList<GridPos> ClearCells(IEnumerable<GridPos> positions)
        {
            var toClear = new Grid<bool>(Width, Height);

            foreach (GridPos pos in positions)
                toClear[pos] = true;

            var cleared = new List<GridPos>();

            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    var pos = new GridPos(x, y);

                    if (!toClear[pos] || IsEmpty(pos))
                        continue;

                    SetCell(pos, Cell.Empty);
                    cleared.Add(pos);
                }
            }

            return cleared;
        }

        #endregion
    }
}
