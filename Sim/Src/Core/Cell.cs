using System;

namespace Squarely.Simulation
{
    /// <summary>
    /// The content of one board cell, identified by a string type id. A null type id means empty,
    /// so a default Cell is an empty cell.
    /// Per-cell effects that are independent of the type (e.g. fire) will live in separate board layers.
    /// </summary>
    public readonly struct Cell : IEquatable<Cell>
    {
        public const string BlockType = "block";

        public static readonly Cell Empty = default;
        public static readonly Cell Block = new Cell(BlockType);

        public readonly string TypeId;

        public bool IsEmpty => TypeId == null;

        public Cell(string typeId)
        {
            if (string.IsNullOrEmpty(typeId))
                throw new ArgumentException("A filled cell needs a type id. Use Cell.Empty for empty cells.", nameof(typeId));

            TypeId = typeId;
        }

        #region Equality

        public bool Equals(Cell other)
        {
            return string.Equals(TypeId, other.TypeId, StringComparison.Ordinal);
        }

        public override bool Equals(object obj)
        {
            return obj is Cell other && Equals(other);
        }

        public override int GetHashCode()
        {
            return TypeId == null ? 0 : StringComparer.Ordinal.GetHashCode(TypeId);
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
            return IsEmpty ? "Empty" : TypeId;
        }
    }
}
