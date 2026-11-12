using System;
using System.Collections.Generic;
using UnityEngine;

namespace Squarely.Presentation
{
    /// <summary>Plays a sound cue and ends at once. Does nothing if the views have no sound player.</summary>
    [Serializable]
    public sealed class PlaySound : CueStep
    {
        [SerializeField] private SoundCue _cue;

        [Tooltip("Where the level comes from. A higher level raises the pitch by the cue's pitch step.")]
        [SerializeField] private SoundLevelSource _levelSource = SoundLevelSource.None;

        [Tooltip("The step plays only when the level is in this range. No limit set: it always plays.")]
        [SerializeField] private SoundLevelRange _levelRange = new SoundLevelRange();

        public override Awaitable Play(CueTarget target, PlaybackContext context)
        {
            if (_cue != null && context.Views.Sound != null && !context.IsStopped)
            {
                var input = SoundLevelInput.From(target, context.Events);
                int level = input.Select(_levelSource);

                if (_levelRange.Contains(level))
                    context.Views.Sound.Play(_cue, level);
            }

            return Completed();
        }

        public override void Snap(CueTarget target, PlaybackContext context)
        {
        }

        public override void CollectSounds(float startSeconds, SoundLevelInput input, List<ScheduledSound> sounds)
        {
            if (_cue == null)
                return;

            int level = input.Select(_levelSource);

            if (_levelRange.Contains(level))
                sounds.Add(new ScheduledSound(startSeconds, _cue, level));
        }
    }
}
