using System.Collections.Generic;
using Squarely.Simulation;

namespace Squarely.Presentation
{
    /// <summary>The numbers a sound level can come from, for one cue run.</summary>
    public readonly struct SoundLevelInput
    {
        public int Count { get; }

        public int ClearCount { get; }

        public int ComboLevel { get; }

        public SoundLevelInput(int count, int clearCount, int comboLevel)
        {
            Count = count;
            ClearCount = clearCount;
            ComboLevel = comboLevel;
        }

        /// <summary>Reads the numbers from a target's count and the events of the command.</summary>
        public static SoundLevelInput From(CueTarget target, IReadOnlyList<ISimEvent> events)
        {
            int clearCount = 0;
            int comboLevel = 0;

            foreach (ISimEvent simEvent in events)
            {
                if (simEvent is PatternsClearedEvent cleared)
                    clearCount += cleared.Matches.Count;
                else if (simEvent is ComboChangedEvent combo)
                    comboLevel = combo.Current;
            }

            return new SoundLevelInput(target.HasCount ? target.CountTo : 0, clearCount, comboLevel);
        }

        /// <summary>The level a source gives.</summary>
        public int Select(SoundLevelSource source)
        {
            switch (source)
            {
                case SoundLevelSource.Count:
                    return Count;
                case SoundLevelSource.ClearCount:
                    return ClearCount;
                case SoundLevelSource.ComboLevel:
                    return ComboLevel;
                default:
                    return 0;
            }
        }
    }
}
