namespace Squarely.Simulation
{
    public sealed class PiecePlacedEvent : ISimEvent
    {
        public PieceInstance Piece { get; }

        /// <summary>The hand slot the piece was taken from.</summary>
        public int Slot { get; }

        public GridPos Origin { get; }

        public PiecePlacedEvent(PieceInstance piece, int slot, GridPos origin)
        {
            Piece = piece;
            Slot = slot;
            Origin = origin;
        }
    }
}
