using System.Collections.Generic;
using Squarely.Simulation;
using UnityEngine;

namespace Squarely.Presentation
{
    /// <summary>
    /// Empties the piece's hand slot, spawns its cells on the board and plays the placement cues on them.
    /// If the same command clears patterns, the cells appear without cues, so the clear plays right away.
    /// On every placement it also plays the board wave on the board's other filled cells that stay (see <see cref="WaveCells"/>).
    /// </summary>
    public sealed class PiecePlacedHandler : SimEventHandler<PiecePlacedEvent>
    {
        private readonly CueSequence _cues;
        private readonly CueSequence _boardWaveCues;

        /// <param name="cues">Played on the placed piece's cells.</param>
        /// <param name="boardWaveCues">Played on the board's other filled cells that the move does not clear.</param>
        public PiecePlacedHandler(CueSequence cues, CueSequence boardWaveCues)
        {
            _cues = cues;
            _boardWaveCues = boardWaveCues;
        }

        protected override void Handle(PiecePlacedEvent simEvent, PlaybackContext context)
        {
            PlaybackViews views = context.Views;
            PieceShape shape = simEvent.Piece.Shape;

            context.SnapRunsOn(views.Hand.GetSlotView(simEvent.Slot).Cells);
            views.Hand.SetPiece(simEvent.Slot, null);

            // Taken before the piece spawns, so the wave leaves the placed cells to the placement cues.
            List<GridPos> filled = views.Board.GetFilledSlots();

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

            PlayBoardWave(WaveCells(filled, context.Events), center, context);
        }

        /// <summary>
        /// The cells the board wave plays on: <paramref name="filled"/> without the cells that a
        /// <see cref="PatternsClearedEvent"/> in <paramref name="events"/> clears, in the same order.
        /// No later event of the command takes over these cells, so a clear never cuts the wave short.
        /// </summary>
        /// <param name="filled">The board's filled cells before the piece is placed.</param>
        /// <param name="events">All events of the command.</param>
        public static List<GridPos> WaveCells(IReadOnlyList<GridPos> filled, IReadOnlyList<ISimEvent> events)
        {
            var cleared = new HashSet<GridPos>();

            foreach (ISimEvent simEvent in events)
            {
                if (simEvent is PatternsClearedEvent clear)
                    cleared.UnionWith(clear.ClearedCells);
            }

            var wave = new List<GridPos>(filled.Count);

            foreach (GridPos pos in filled)
            {
                if (!cleared.Contains(pos))
                    wave.Add(pos);
            }

            return wave;
        }

        private void PlayBoardWave(List<GridPos> positions, Vector3 center, PlaybackContext context)
        {
            var cells = new List<CellView>(positions.Count);

            foreach (GridPos pos in positions)
                cells.Add(context.Views.Board.GetView(pos));

            context.Play(_boardWaveCues, CueTarget.ForCells(cells, positions, center));
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
