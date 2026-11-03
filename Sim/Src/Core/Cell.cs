using System;

namespace Squarely.Simulation
{
    /// <summary>
    /// The content of one board cell. Type 0 means empty.
    /// Per-cell effects that are independent of the type (e.g. fire) will live in separate board layers.
    /// </summary>
    public readonly struct Cell : IEquatable<Cell>
    {
        public const int EmptyType = 0;
        public const int BlockType = 1;

        public static readonly Cell Empty = new Cell(EmptyType);
        public static readonly Cell Block = new Cell(BlockType);

        public readonly int Type;

        public Cell(int type)
        {
            Type = type;
        }

        public bool IsEmpty => Type == EmptyType;

        #region Equality

        public bool Equals(Cell other)
        {
            return Type == other.Type;
        }

        public override bool Equals(object obj)
        {
            return obj is Cell other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Type;
        }

        public static bool operator ==(Cell a, Cell b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(Cell a, Cell b)
        {
            return !a.Equals(b);
        }

        #endregion

        public override string ToString()
        {
            return IsEmpty ? "Empty" : $"Type {Type}";
        }
    }
}
