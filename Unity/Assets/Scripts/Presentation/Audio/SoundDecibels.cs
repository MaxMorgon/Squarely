using UnityEngine;

namespace Squarely.Presentation
{
    /// <summary>Conversions between volume and decibels.</summary>
    public static class SoundDecibels
    {
        /// <summary>The level of a muted mixer group, in dB.</summary>
        public const float Off = -80f;

        /// <summary>The level of a mixer group that plays, in dB.</summary>
        public const float On = 0f;

        /// <summary>The mixer level of a toggle.</summary>
        public static float ForToggle(bool isOn)
        {
            return isOn ? On : Off;
        }

        /// <summary>The decibel value of a linear volume from 0 to 1. A volume of 0 gives <see cref="Off"/>.</summary>
        public static float FromLinear(float volume)
        {
            if (volume <= 0.0001f)
                return Off;

            return Mathf.Max(Off, 20f * Mathf.Log10(volume));
        }
    }
}
