using System;

namespace Squarely.Simulation
{
    /// <summary>
    /// The player's pieces. A fixed number of slots; each holds a piece or is empty (null).
    /// </summary>
    public sealed class Hand
    {
        private readonly PieceShape[] _slots;

        public Hand(int size)
        {
            if (size <= 0)
                throw new ArgumentOutOfRangeException(nameof(size), "Hand size must be positive.");

            _slots = new PieceShape[size];
        }

        public int Size => _slots.Length;

        public bool IsValidSlot(int slot)
        {
            return slot >= 0 && slot < _slots.Length;
        }

        public PieceShape Get(int slot)
        {
            return _slots[slot];
        }

        public bool IsEmpty(int slot)
        {
            return _slots[slot] == null;
        }

        public void Set(int slot, PieceShape piece)
        {
            _slots[slot] = piece;
        }

        public PieceShape Take(int slot)
        {
            PieceShape piece = _slots[slot];
            _slots[slot] = null;
            return piece;
        }
    }
}
