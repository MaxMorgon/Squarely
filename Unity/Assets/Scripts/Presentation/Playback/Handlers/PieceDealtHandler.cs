using System.Collections.Generic;
using Squarely.Simulation;

namespace Squarely.Presentation
{
    /// <summary>Shows the dealt piece in its hand slot and plays the deal cues on its cells.</summary>
    public sealed class PieceDealtHandler : SimEventHandler<PieceDealtEvent>
    {
        private readonly CueSequence _cues;

        public PieceDealtHandler(CueSequence cues)
        {
            _cues = cues;
        }

        protected override void Handle(PieceDealtEvent simEvent, PlaybackContext context)
        {
            HandView hand = context.Views.Hand;
            context.SnapRunsOn(hand.GetSlotView(simEvent.Slot).Cells);
            hand.SetPiece(simEvent.Slot, simEvent.Piece);

            PieceView slotView = hand.GetSlotView(simEvent.Slot);

            // A copy: the slot view reuses its list for the next piece.
            var cells = new List<CellView>(slotView.Cells);
            CueTarget target = CueTarget.ForCells(cells, simEvent.Piece.Shape.Offsets, slotView.transform.position);
            context.Play(_cues, target);
        }
    }
}
