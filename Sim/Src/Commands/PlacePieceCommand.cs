namespace Squarely.Simulation
{
    /// <summary>
    /// Place the piece in a hand slot so its bottom-left corner is at Origin.
    /// </summary>
    public sealed class PlacePieceCommand : ISimCommand
    {
        public PlacePieceCommand(int slot, GridPos origin)
        {
            Slot = slot;
            Origin = origin;
        }

        public int Slot { get; }

        public GridPos Origin { get; }

        public CommandResult Execute(Sim sim)
        {
            return sim.PlacePiece(Slot, Origin);
        }

        public override string ToString()
        {
            return $"Place slot {Slot} at {Origin}";
        }
    }
}
