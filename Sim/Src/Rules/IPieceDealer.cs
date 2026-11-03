namespace Squarely.Simulation
{
    /// <summary>
    /// Decides which piece goes into an empty hand slot.
    /// Gets the board and hand so smarter dealers (e.g. "guarantee one piece fits") can inspect them.
    /// Must only use the given random for any randomness, to stay deterministic.
    /// </summary>
    public interface IPieceDealer
    {
        PieceShape Deal(Board board, Hand hand, DeterministicRandom random);
    }
}
