namespace Squarely.Simulation
{
    /// <summary>
    /// Read-only view of the hand. Slots give the view a stable position for each piece.
    /// </summary>
    public interface IReadOnlyHand
    {
        int Size { get; }

        /// <summary>True when every slot is empty.</summary>
        bool IsEmpty { get; }

        /// <summary>The piece in a slot, or null if the slot is empty.</summary>
        PieceInstance GetSlot(int slot);

        bool IsSlotEmpty(int slot);

        /// <summary>The slot holding the given piece instance, or -1 if it is not in the hand.</summary>
        int FindSlot(int instanceId);
    }
}
