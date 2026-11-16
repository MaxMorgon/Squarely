using Squarely.Simulation;

namespace Squarely.Presentation
{
    /// <summary>
    /// The look variant of a dealt piece: a scrambled number from its instance id.
    /// It is view-only and stable, so a piece keeps its look from hand to board and replays look the same.
    /// </summary>
    public static class PieceVariant
    {
        /// <summary>A non-negative variant number for the piece. Views map it to a look with a modulo.</summary>
        public static int Of(PieceInstance piece)
        {
            uint hash = unchecked((uint)piece.InstanceId * 2654435761u);
            return (int)(hash >> 8);
        }
    }
}
