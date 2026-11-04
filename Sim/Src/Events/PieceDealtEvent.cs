namespace Squarely.Simulation
{
    public sealed class PieceDealtEvent : ISimEvent
    {
        public PieceInstance Piece { get; }

        public int Slot { get; }

        public PieceDealtEvent(PieceInstance piece, int slot)
        {
            Piece = piece;
            Slot = slot;
        }
    }
}
