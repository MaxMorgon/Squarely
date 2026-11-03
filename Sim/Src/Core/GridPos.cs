using System;

namespace Squarely.Simulation
{
    /// <summary>
    /// A cell coordinate on the board. Origin is bottom-left; X grows right, Y grows up.
    /// </summary>
    public readonly struct GridPos : IEquatable<GridPos>
    {
        public readonly int X;
        public readonly int Y;

        public GridPos(int x, int y)
        {
            X = x;
            Y = y;
        }

        #region Operators

        public static GridPos operator +(GridPos a, GridPos b)
        {
            return new GridPos(a.X + b.X, a.Y + b.Y);
        }

        public static bool operator ==(GridPos a, GridPos b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(GridPos a, GridPos b)
        {
            return !a.Equals(b);
        }

        #endregion

        #region Equality

        public bool Equals(GridPos other)
        {
            return X == other.X && Y == other.Y;
        }

        public override bool Equals(object obj)
        {
            return obj is GridPos other && Equals(other);
        }

        public override int GetHashCode()
        {
            return (X * 397) ^ Y;
        }

        #endregion

        public override string ToString()
        {
            return $"({X}, {Y})";
        }
    }
}
