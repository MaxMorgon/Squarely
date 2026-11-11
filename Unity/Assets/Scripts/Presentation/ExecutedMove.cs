using System.Collections.Generic;
using Squarely.Simulation;
using UnityEngine;

namespace Squarely.Presentation
{
    /// <summary>A move the game executed: the state before it, the events it gave and where the pointer dropped the piece.</summary>
    public sealed class ExecutedMove
    {
        /// <summary>A copy of the sim as it was before the move. Null if nobody needed it.</summary>
        public Sim SimBefore { get; }

        public IReadOnlyList<ISimEvent> Events { get; }

        /// <summary>See <see cref="GameController.Place"/>. Null without a pointer.</summary>
        public Vector2? DropPoint { get; }

        public ExecutedMove(Sim simBefore, IReadOnlyList<ISimEvent> events, Vector2? dropPoint)
        {
            SimBefore = simBefore;
            Events = events;
            DropPoint = dropPoint;
        }
    }
}
