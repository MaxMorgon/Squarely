using System;
using System.Collections.Generic;

namespace Squarely.Simulation
{
    /// <summary>
    /// A set of cell conditions that, when all met, count as a match: full rows and columns,
    /// pixel-art images, small shapes like a smiley. Matched cells are cleared.
    /// A pattern is either anchored to one board position or can match anywhere.
    /// </summary>
    public sealed class BoardPattern
    {
        private readonly PatternCell[] _cells;

        /// <param name="anchor">Fixed board position of the pattern origin, or null to match anywhere.</param>
        public BoardPattern(string id, IEnumerable<PatternCell> cells, int points, GridPos? anchor)
        {
            if (string.IsNullOrEmpty(id))
                throw new ArgumentException("Pattern id is required.", nameof(id));

            if (cells == null)
                throw new ArgumentNullException(nameof(cells));

            Id = id;
            Points = points;
            Anchor = anchor;
            _cells = new List<PatternCell>(cells).ToArray();

            if (_cells.Length == 0)
                throw new ArgumentException($"Pattern '{id}' has no cells.", nameof(cells));

            foreach (PatternCell cell in _cells)
            {
                if (cell.Offset.X < 0 || cell.Offset.Y < 0)
                    throw new ArgumentException($"Pattern '{id}' has a negative offset {cell.Offset}.", nameof(cells));

                Width = Math.Max(Width, cell.Offset.X + 1);
                Height = Math.Max(Height, cell.Offset.Y + 1);
            }
        }

        public string Id { get; }

        public IReadOnlyList<PatternCell> Cells => _cells;

        /// <summary>Points awarded for completing this pattern.</summary>
        public int Points { get; }

        public GridPos? Anchor { get; }

        public int Width { get; }

        public int Height { get; }

        #region Factories

        /// <summary>
        /// Builds a pattern from text. Rows are separated by '/', top row first.
        /// '#' = must be filled, 'o' = must be empty, '.' = not part of the pattern.
        /// </summary>
        public static BoardPattern FromText(string id, string text, int points, GridPos? anchor)
        {
            if (string.IsNullOrEmpty(text))
                throw new ArgumentException($"Pattern '{id}' has empty text.", nameof(text));

            string[] rows = text.Split('/');
            var cells = new List<PatternCell>();

            for (int rowIndex = 0; rowIndex < rows.Length; rowIndex++)
            {
                int y = rows.Length - 1 - rowIndex;
                string row = rows[rowIndex];

                for (int x = 0; x < row.Length; x++)
                {
                    char symbol = row[x];
                    var offset = new GridPos(x, y);

                    if (symbol == '#')
                        cells.Add(new PatternCell(offset, FilledCondition.Instance));
                    else if (symbol == 'o')
                        cells.Add(new PatternCell(offset, EmptyCondition.Instance));
                    else if (symbol != '.')
                        throw new FormatException($"Pattern '{id}' has invalid character '{symbol}' in '{text}'.");
                }
            }

            return new BoardPattern(id, cells, points, anchor);
        }

        public static BoardPattern Row(int y, int boardWidth, int points)
        {
            var cells = new List<PatternCell>();

            for (int x = 0; x < boardWidth; x++)
                cells.Add(new PatternCell(new GridPos(x, 0), FilledCondition.Instance));

            return new BoardPattern($"row_{y}", cells, points, new GridPos(0, y));
        }

        public static BoardPattern Column(int x, int boardHeight, int points)
        {
            var cells = new List<PatternCell>();

            for (int y = 0; y < boardHeight; y++)
                cells.Add(new PatternCell(new GridPos(0, y), FilledCondition.Instance));

            return new BoardPattern($"column_{x}", cells, points, new GridPos(x, 0));
        }

        #endregion

        public override string ToString()
        {
            return Id;
        }
    }
}
