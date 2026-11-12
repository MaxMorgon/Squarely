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

        /// <summary>The combo that the command ended, or 0 if the combo did not drop to 0.</summary>
        public int ComboLostLevel { get; }

        /// <summary>The start value of the target's count.</summary>
        public int CountFrom { get; }

        public SoundLevelInput(int count, int clearCount, int comboLevel, int comboLostLevel = 0, int countFrom = 0)
        {
            Count = count;
            ClearCount = clearCount;
            ComboLevel = comboLevel;
            ComboLostLevel = comboLostLevel;
            CountFrom = countFrom;
        }

        /// <summary>Reads the numbers from a target's count and the events of the command.</summary>
        public static SoundLevelInput From(CueTarget target, IReadOnlyList<ISimEvent> events)
        {
            int clearCount = 0;
            int comboLevel = 0;
            int comboLostLevel = 0;

            foreach (ISimEvent simEvent in events)
            {
                if (simEvent is PatternsClearedEvent cleared)
                {
                    clearCount += cleared.Matches.Count;
                }
                else if (simEvent is ComboChangedEvent combo)
                {
                    comboLevel = combo.Current;
                    comboLostLevel = combo.Current == 0 ? combo.Previous : 0;
                }
            }

            int count = target.HasCount ? target.CountTo : 0;
            int countFrom = target.HasCount ? target.CountFrom : 0;

            return new SoundLevelInput(count, clearCount, comboLevel, comboLostLevel, countFrom);
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
                case SoundLevelSource.ComboLost:
                    return ComboLostLevel;
                default:
                    return 0;
            }
        }
    }
}
