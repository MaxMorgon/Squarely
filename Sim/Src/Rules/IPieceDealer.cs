namespace Squarely.Simulation
{
    /// <summary>
    /// Decides which piece goes into an empty hand slot.
    /// Gets the sim so smarter dealers (e.g. "guarantee one piece fits") can inspect the board and hand.
    /// Must only use the given random for any randomness, to stay deterministic.
    /// Must hold no mutable state: one instance is shared by a sim and its preview clones.
    /// </summary>
    public interface IPieceDealer
    {
        /// <param name="sim">The sim mid-refill: the hand may be partly filled already.</param>
        PieceShape Deal(Sim sim, DeterministicRandom random);
    }
}
