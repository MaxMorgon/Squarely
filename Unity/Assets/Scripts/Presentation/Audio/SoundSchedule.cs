using System.Collections.Generic;

namespace Squarely.Presentation
{
    /// <summary>Lists the sounds a cue sequence starts, with the times its steps give them.</summary>
    public static class SoundSchedule
    {
        /// <summary>The sounds of the steps, in play order. A step starts when the one before it ends.</summary>
        public static List<ScheduledSound> Build(IReadOnlyList<CueStep> steps, SoundLevelInput input)
        {
            var sounds = new List<ScheduledSound>();
            float time = 0f;

            foreach (CueStep step in steps)
            {
                if (step == null)
                    continue;

                step.CollectSounds(time, input, sounds);
                time += step.PlannedSeconds;
            }

            return sounds;
        }
    }
}
