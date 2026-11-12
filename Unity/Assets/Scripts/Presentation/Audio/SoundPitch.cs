using UnityEngine;

namespace Squarely.Presentation
{
    /// <summary>Pitch math for sounds.</summary>
    public static class SoundPitch
    {
        private const float SemitonesPerOctave = 12f;

        /// <summary>
        /// The pitch factor of a play: a point in the variation range, raised by the step for the level.
        /// </summary>
        /// <param name="variationMin">Lowest pitch factor of the random variation.</param>
        /// <param name="variationMax">Highest pitch factor of the random variation.</param>
        /// <param name="randomT">A value from 0 to 1 that picks the point in the variation range.</param>
        /// <param name="stepSemitones">Pitch raise per level, in semitones.</param>
        /// <param name="level">Level 0 plays at the base pitch. A negative level counts as 0.</param>
        /// <param name="maxLevel">Highest level that raises the pitch. A higher level counts as this level.</param>
        public static float Calculate(float variationMin, float variationMax, float randomT, float stepSemitones, int level, int maxLevel)
        {
            float variation = Mathf.Lerp(variationMin, variationMax, Mathf.Clamp01(randomT));
            int cappedLevel = Mathf.Clamp(level, 0, Mathf.Max(0, maxLevel));
            float semitones = stepSemitones * cappedLevel;
            return variation * Mathf.Pow(2f, semitones / SemitonesPerOctave);
        }
    }
}
