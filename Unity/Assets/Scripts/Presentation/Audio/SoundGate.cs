using UnityEngine;

namespace Squarely.Presentation
{
    /// <summary>Decides if a sound may start, from its cooldown and voice limit.</summary>
    public static class SoundGate
    {
        /// <param name="now">The current time, in seconds.</param>
        /// <param name="lastPlayTime">When the sound last started, in seconds, or negative infinity if it never did.</param>
        /// <param name="cooldown">Seconds that must pass after the last start.</param>
        /// <param name="activeVoices">Voices of this sound that play now.</param>
        /// <param name="maxVoices">Voices of this sound that may play at once. At least 1.</param>
        public static bool CanPlay(double now, double lastPlayTime, float cooldown, int activeVoices, int maxVoices)
        {
            if (now - lastPlayTime < cooldown)
                return false;

            return activeVoices < Mathf.Max(1, maxVoices);
        }
    }
}
