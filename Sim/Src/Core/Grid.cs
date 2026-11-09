using System;

namespace Squarely.Simulation
{
    /// <summary>
    /// A fixed-size rectangular grid holding one value per position. Positions outside the grid throw.
    /// </summary>
    public sealed class Grid<T>
    {
        private readonly T[] _items;

        public int Width { get; }

        public int Height { get; }

        public Grid(int width, int height)
        {
            if (width <= 0)
                throw new ArgumentOutOfRangeException(nameof(width), "Grid width must be positive.");

            if (height <= 0)
                throw new ArgumentOutOfRangeException(nameof(height), "Grid height must be positive.");

            Width = width;
            Height = height;
            _items = new T[width * height];
        }

        public T this[GridPos pos]
        {
            get => _items[IndexOf(pos)];
            set => _items[IndexOf(pos)] = value;
        }

        public bool IsWithinBounds(GridPos pos)
        {
            return pos.X >= 0 && pos.X < Width && pos.Y >= 0 && pos.Y < Height;
        }

        /// <summary>A copy with the same values. Reference-type values are shared, not cloned.</summary>
        public Grid<T> Clone()
        {
            var copy = new Grid<T>(Width, Height);
            Array.Copy(_items, copy._items, _items.Length);
            return copy;
        }

        private int IndexOf(GridPos pos)
        {
            if (!IsWithinBounds(pos))
                throw new ArgumentOutOfRangeException(nameof(pos), $"Position {pos} is outside the {Width}x{Height} grid.");

            return pos.Y * Width + pos.X;
        }
    }
}
