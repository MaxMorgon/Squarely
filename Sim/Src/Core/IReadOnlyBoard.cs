namespace Squarely.Simulation
{
    /// <summary>
    /// Read-only view of the board, for the view layer and rules. Mutation is internal to the sim.
    /// </summary>
    public interface IReadOnlyBoard
    {
        int Width { get; }

        int Height { get; }

        bool IsWithinBounds(GridPos pos);

        Cell GetCell(GridPos pos);

        bool IsEmpty(GridPos pos);

        bool CanPlace(PieceShape shape, GridPos origin);

        bool CanPlaceAnywhere(PieceShape shape);
    }
}
