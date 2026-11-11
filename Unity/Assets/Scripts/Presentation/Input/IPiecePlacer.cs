using Squarely.Simulation;
using UnityEngine;

namespace Squarely.Presentation
{
    /// <summary>What drag input needs from the game: whether moves are still accepted, and placing pieces.</summary>
    public interface IPiecePlacer
    {
        bool IsResolved { get; }

        /// <summary>Whether placing the piece at the origin is a legal move.</summary>
        bool CanPlace(int pieceInstanceId, GridPos origin);

        /// <param name="dropPoint">Where the pointer dropped the piece, in board cell coordinates (<see cref="BoardSpace.CellCoordinates"/>), or null.</param>
        void Place(int pieceInstanceId, GridPos origin, Vector2? dropPoint);
    }
}
