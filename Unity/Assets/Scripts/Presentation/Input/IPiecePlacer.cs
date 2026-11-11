using Squarely.Simulation;

namespace Squarely.Presentation
{
    /// <summary>What drag input needs from the game: whether moves are still accepted, and placing pieces.</summary>
    public interface IPiecePlacer
    {
        bool IsResolved { get; }

        /// <summary>Whether placing the piece at the origin is a legal move.</summary>
        bool CanPlace(int pieceInstanceId, GridPos origin);

        void Place(int pieceInstanceId, GridPos origin);
    }
}
