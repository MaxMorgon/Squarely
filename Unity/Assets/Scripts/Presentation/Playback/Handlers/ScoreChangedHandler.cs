using Squarely.Simulation;

namespace Squarely.Presentation
{
    /// <summary>Plays the score cues, counting the score label from what it shows to the new total.</summary>
    public sealed class ScoreChangedHandler : SimEventHandler<ScoreChangedEvent>
    {
        private readonly CueSequence _cues;

        public ScoreChangedHandler(CueSequence cues)
        {
            _cues = cues;
        }

        protected override void Handle(ScoreChangedEvent simEvent, PlaybackContext context)
        {
            ScoreView score = context.Views.Score;
            CueTarget target = CueTarget.ForCount(score.Score, simEvent.Total, score.SetScore, score.ScoreLabel, context.LastEffectPosition);

            // The count's end value must reach the label even if the cues don't count.
            context.Play(_cues, target, () => score.SetScore(simEvent.Total));
        }
    }
}
