using System.Collections.Generic;
using Squarely.Simulation;
using UnityEngine;

namespace Squarely.Presentation
{
    /// <summary>
    /// Empties the piece's hand slot, spawns its cells on the board and plays the placement cues on them.
    /// If the same command clears patterns, the cells appear without cues, so the clear plays right away.
    /// On every placement it also plays the board wave on the board's filled cells that stay (see <see cref="WaveCells"/>).
    /// </summary>
    public sealed class PiecePlacedHandler : SimEventHandler<PiecePlacedEvent>
    {
        private readonly CueSequence _cues;
        private readonly CueSequence _boardWaveCues;

        /// <param name="cues">Played on the placed piece's cells when the move clears nothing.</param>
        /// <param name="boardWaveCues">Played on the board's filled cells that the move does not clear.</param>
        public PiecePlacedHandler(CueSequence cues, CueSequence boardWaveCues)
        {
            _cues = cues;
            _boardWaveCues = boardWaveCues;
        }

        protected override void Handle(PiecePlacedEvent simEvent, PlaybackContext context)
        {
            PlaybackViews views = context.Views;
            PieceShape shape = simEvent.Piece.Shape;
            HashSet<GridPos> cleared = ClearedCells(context.Events);

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

            if (cleared.Count == 0)
                context.Play(_cues, CueTarget.ForCells(cells, positions, center));

            PlayBoardWave(WaveCells(views.Board.GetFilledSlots(), positions, cleared), center, context);
        }

        /// <summary>
        /// Every cell that a <see cref="PatternsClearedEvent"/> in <paramref name="events"/> clears.
        /// Empty if the command clears nothing.
        /// </summary>
        /// <param name="events">All events of the command.</param>
        internal static HashSet<GridPos> ClearedCells(IReadOnlyList<ISimEvent> events)
        {
            var cleared = new HashSet<GridPos>();

            foreach (ISimEvent simEvent in events)
            {
                if (simEvent is PatternsClearedEvent clear)
                    cleared.UnionWith(clear.ClearedCells);
            }

            return cleared;
        }

        /// <summary>
        /// The cells the board wave plays on, in the order of <paramref name="filled"/>: every filled cell the command
        /// does not clear. If it clears nothing, the placed cells are left out too, since they play the placement cues.
        /// No later event of the command takes over these cells, so a clear never cuts the wave short.
        /// </summary>
        /// <param name="filled">The board's filled cells after the piece is placed.</param>
        /// <param name="placed">The placed piece's cells.</param>
        /// <param name="cleared">The cells the command clears (see <see cref="ClearedCells"/>).</param>
        internal static List<GridPos> WaveCells(IReadOnlyList<GridPos> filled, IReadOnlyList<GridPos> placed, HashSet<GridPos> cleared)
        {
            bool clears = cleared.Count > 0;
            var wave = new List<GridPos>(filled.Count);

            foreach (GridPos pos in filled)
            {
                if (cleared.Contains(pos))
                    continue;

                if (!clears && Contains(placed, pos))
                    continue;

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

        private static bool Contains(IReadOnlyList<GridPos> positions, GridPos pos)
        {
            foreach (GridPos other in positions)
            {
                if (other == pos)
                    return true;
            }

            return false;
        }
    }
}
