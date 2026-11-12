namespace Squarely.Presentation
{
    /// <summary>Decides what <see cref="MusicPlayer.Play"/> does with a requested track.</summary>
    public static class MusicSwitchRule
    {
        public enum Action
        {
            /// <summary>The track already plays and is not fading out. Do nothing.</summary>
            Keep,

            /// <summary>The track still plays on the active source, fading out. Fade it back in from its volume.</summary>
            FadeActiveBackIn,

            /// <summary>The track still plays on the other source, fading out. Make that source active and fade it back in from its volume.</summary>
            FadeOtherBackIn,

            /// <summary>Start the track on the other source from the beginning.</summary>
            StartNew
        }

        /// <param name="activeHasTrack">The active source has the requested track.</param>
        /// <param name="activeIsPlaying">The active source plays now.</param>
        /// <param name="activeTarget">The fade target of the active source.</param>
        /// <param name="otherHasTrack">The other source has the requested track.</param>
        /// <param name="otherIsPlaying">The other source plays now.</param>
        public static Action Choose(bool activeHasTrack, bool activeIsPlaying, float activeTarget, bool otherHasTrack, bool otherIsPlaying)
        {
            if (activeHasTrack && activeIsPlaying)
                return activeTarget > 0f ? Action.Keep : Action.FadeActiveBackIn;

            if (otherHasTrack && otherIsPlaying)
                return Action.FadeOtherBackIn;

            return Action.StartNew;
        }
    }
}
