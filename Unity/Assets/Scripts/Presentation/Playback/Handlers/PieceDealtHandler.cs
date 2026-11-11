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
            hand.SetPiece(simEvent.Slot, simEvent.Piece);

            PieceView slotView = hand.GetSlotView(simEvent.Slot);
            CueTarget target = CueTarget.ForCells(slotView.Cells, simEvent.Piece.Shape.Offsets, slotView.transform.position);
            context.Play(_cues, target);
        }
    }
}
