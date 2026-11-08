using System;
using Squarely.Simulation;

namespace Squarely.Presentation
{
    /// <summary>
    /// A grid of slots, each holding one item or nothing.
    /// </summary>
    public sealed class SlotGrid<T> where T : class
    {
        private readonly T[] _slots;

        public int Width { get; }

        public int Height { get; }

        public SlotGrid(int width, int height)
        {
            if (width <= 0 || height <= 0)
                throw new ArgumentOutOfRangeException(nameof(width), "Grid size must be positive.");

            Width = width;
            Height = height;
            _slots = new T[width * height];
        }

        public bool Contains(GridPos pos)
        {
            return pos.X >= 0 && pos.X < Width && pos.Y >= 0 && pos.Y < Height;
        }

        /// <summary>The item in a slot, or null if the slot is empty.</summary>
        public T Get(GridPos pos)
        {
            return _slots[IndexOf(pos)];
        }

        /// <summary>Puts an item in an empty slot.</summary>
        public void Set(GridPos pos, T item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            int index = IndexOf(pos);

            if (_slots[index] != null)
                throw new InvalidOperationException($"Slot {pos} is already occupied.");

            _slots[index] = item;
        }

        /// <summary>Empties a slot and returns what it held, or null if it was empty.</summary>
        public T Detach(GridPos pos)
        {
            int index = IndexOf(pos);
            T item = _slots[index];
            _slots[index] = null;
            return item;
        }

        private int IndexOf(GridPos pos)
        {
            if (!Contains(pos))
                throw new ArgumentOutOfRangeException(nameof(pos), $"{pos} is outside the {Width}x{Height} grid.");

            return pos.Y * Width + pos.X;
        }
    }
}
