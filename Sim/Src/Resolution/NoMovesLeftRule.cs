namespace Squarely.Simulation
{
    /// <summary>
    /// Lost when no piece in the hand fits anywhere on the board.
    /// </summary>
    public sealed class NoMovesLeftRule : IResolutionRule
    {
        public GameResolution Check(Sim sim)
        {
            for (int slot = 0; slot < sim.Hand.Size; slot++)
            {
                PieceInstance piece = sim.Hand.GetSlot(slot);

                if (piece != null && sim.Board.CanPlaceAnywhere(piece.Shape))
                    return null;
            }

            return CreateResolution();
        }

        /// <summary>The resolution of a game that ends because no move is left.</summary>
        internal static GameResolution CreateResolution()
        {
            return new GameResolution(GameOutcome.Lost, RuleIds.NoMovesLeft);
        }
    }
}
