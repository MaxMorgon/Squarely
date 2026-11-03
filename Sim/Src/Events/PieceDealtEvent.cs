namespace Squarely.Simulation
{
    public sealed class PieceDealtEvent : ISimEvent
    {
        public PieceDealtEvent(PieceInstance piece, int slot)
        {
            Piece = piece;
            Slot = slot;
        }

        public PieceInstance Piece { get; }

        public int Slot { get; }
    }
}
