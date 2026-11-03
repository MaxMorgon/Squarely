namespace Squarely.Simulation
{
    /// <summary>
    /// Place a piece from the hand, identified by its instance id, with its bottom-left corner at Origin.
    /// </summary>
    public sealed class PlacePieceCommand : ISimCommand
    {
        public PlacePieceCommand(int pieceInstanceId, GridPos origin)
        {
            PieceInstanceId = pieceInstanceId;
            Origin = origin;
        }

        public int PieceInstanceId { get; }

        public GridPos Origin { get; }

        public CommandResult Execute(Sim sim)
        {
            return sim.PlacePiece(PieceInstanceId, Origin);
        }

        public override string ToString()
        {
            return $"Place piece #{PieceInstanceId} at {Origin}";
        }
    }
}
