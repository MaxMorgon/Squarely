using System.Collections.Generic;
using Squarely.Simulation;
using UnityEngine;

namespace Squarely.Presentation
{
    /// <summary>
    /// Empties the piece's hand slot, spawns its cells on the board and plays the placement cues on them.
    /// If the same command clears patterns, the cells appear without cues, so the clear plays right away.
    /// </summary>
    public sealed class PiecePlacedHandler : SimEventHandler<PiecePlacedEvent>
    {
        private readonly CueSequence _cues;

        public PiecePlacedHandler(CueSequence cues)
        {
            _cues = cues;
        }

        protected override void Handle(PiecePlacedEvent simEvent, PlaybackContext context)
        {
            PlaybackViews views = context.Views;
            PieceShape shape = simEvent.Piece.Shape;

            context.SnapRunsOn(views.Hand.GetSlotView(simEvent.Slot).Cells);
            views.Hand.SetPiece(simEvent.Slot, null);

            var cells = new List<CellView>(shape.CellCount);
            var positions = new List<GridPos>(shape.CellCount);

            foreach (GridPos offset in shape.Offsets)
            {
                GridPos pos = simEvent.Origin + offset;
                cells.Add(views.Board.Spawn(pos, shape.CellTypeId));
                positions.Add(pos);
            }

            Vector3 center = views.CenterOf(positions);
            context.PlacedCells = positions;
            context.LastEffectPosition = center;

            if (!ClearsPatterns(context.Events))
                context.Play(_cues, CueTarget.ForCells(cells, positions, center));
        }

        private static bool ClearsPatterns(IReadOnlyList<ISimEvent> events)
        {
            foreach (ISimEvent simEvent in events)
            {
                if (simEvent is PatternsClearedEvent)
                    return true;
            }

            return false;
        }
    }
}
