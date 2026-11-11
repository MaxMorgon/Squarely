using Squarely.Simulation;

namespace Squarely.Presentation
{
    /// <summary>Shows the new combo and plays the combo cues on its label.</summary>
    public sealed class ComboChangedHandler : SimEventHandler<ComboChangedEvent>
    {
        private readonly CueSequence _cues;

        public ComboChangedHandler(CueSequence cues)
        {
            _cues = cues;
        }

        protected override void Handle(ComboChangedEvent simEvent, PlaybackContext context)
        {
            ScoreView score = context.Views.Score;
            score.SetCombo(simEvent.Current);
            context.Play(_cues, CueTarget.ForTransform(score.ComboLabel, context.LastEffectPosition));
        }
    }
}
