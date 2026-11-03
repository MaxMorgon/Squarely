using System.Collections.Generic;

namespace Squarely.Simulation
{
    /// <summary>
    /// A set of full rows and columns on the board, in ascending index order.
    /// </summary>
    public sealed class BoardLines
    {
        public static readonly BoardLines None = new BoardLines(new int[0], new int[0]);

        public BoardLines(IReadOnlyList<int> rows, IReadOnlyList<int> columns)
        {
            Rows = rows;
            Columns = columns;
        }

        public IReadOnlyList<int> Rows { get; }

        public IReadOnlyList<int> Columns { get; }

        public int Count => Rows.Count + Columns.Count;

        public bool IsEmpty => Count == 0;
    }
}
