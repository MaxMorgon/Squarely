namespace Squarely.Presentation
{
    /// <summary>A sound that a cue sequence starts, and when.</summary>
    public readonly struct ScheduledSound
    {
        public float Seconds { get; }

        public SoundCue Cue { get; }

        public int Level { get; }

        public ScheduledSound(float seconds, SoundCue cue, int level)
        {
            Seconds = seconds;
            Cue = cue;
            Level = level;
        }
    }
}
