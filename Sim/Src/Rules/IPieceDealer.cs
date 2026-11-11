using System.Collections.Generic;

namespace Squarely.Simulation
{
    /// <summary>
    /// Decides which pieces go into a new hand. Deals the whole hand at once, so a dealer can look at the hand
    /// as a whole (e.g. "at least one piece fits") as well as at the board.
    /// Must only use the given random for any randomness, to stay deterministic.
    /// Must hold no mutable state: one instance is shared by a sim and its preview clones.
    /// </summary>
    public interface IPieceDealer
    {
        /// <param name="sim">The sim at the moment of dealing: the board is final for this turn and the hand is empty.</param>
        /// <returns>One shape per hand slot (<see cref="IReadOnlyHand.Size"/>), in slot order.</returns>
        IReadOnlyList<PieceShape> DealHand(Sim sim, DeterministicRandom random);
    }
}
