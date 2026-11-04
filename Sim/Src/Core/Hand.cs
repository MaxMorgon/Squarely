using System;

namespace Squarely.Simulation
{
    /// <summary>
    /// The player's pieces: a fixed number of slots, each holding a piece instance or empty.
    /// Mutators are internal so only the sim can change the hand.
    /// </summary>
    public sealed class Hand : IReadOnlyHand
    {
        private readonly PieceInstance[] _slots;

        public int Size => _slots.Length;

        public Hand(int size)
        {
            if (size <= 0)
                throw new ArgumentOutOfRangeException(nameof(size), "Hand size must be positive.");

            _slots = new PieceInstance[size];
        }

        #region Reading

        public PieceInstance GetSlot(int slot)
        {
            return _slots[slot];
        }

        public bool IsSlotEmpty(int slot)
        {
            return _slots[slot] == null;
        }

        public int FindSlot(int instanceId)
        {
            for (int slot = 0; slot < _slots.Length; slot++)
            {
                if (_slots[slot] != null && _slots[slot].InstanceId == instanceId)
                    return slot;
            }

            return -1;
        }

        #endregion

        #region Mutation

        /// <summary>A copy with the same slots, for preview clones of the sim. Piece instances are immutable, so they are shared.</summary>
        internal Hand Clone()
        {
            var copy = new Hand(Size);
            Array.Copy(_slots, copy._slots, _slots.Length);
            return copy;
        }

        /// <summary>
        /// Puts the piece in the first empty slot and returns that slot.
        /// </summary>
        internal int Add(PieceInstance piece)
        {
            if (piece == null)
                throw new ArgumentNullException(nameof(piece));

            for (int slot = 0; slot < _slots.Length; slot++)
            {
                if (_slots[slot] != null)
                    continue;

                _slots[slot] = piece;
                return slot;
            }

            throw new InvalidOperationException("The hand is full.");
        }

        internal PieceInstance Take(int instanceId)
        {
            int slot = FindSlot(instanceId);

            if (slot < 0)
                throw new InvalidOperationException($"Piece instance {instanceId} is not in the hand.");

            PieceInstance piece = _slots[slot];
            _slots[slot] = null;
            return piece;
        }

        #endregion
    }
}
