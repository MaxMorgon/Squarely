using System;

namespace Squarely.Presentation
{
    /// <summary>
    /// Decides when the game-over screen may open. It opens once, after the game resolved, the playback that
    /// holds the resolution ended, no celebration is playing, and then a hold time passed.
    /// </summary>
    public sealed class GameEndGate
    {
        private readonly float _hold;

        private bool _isResolved;
        private bool _isPlaybackEnded;
        private bool _isOpen;
        private float _waited;

        /// <param name="hold">Seconds to wait after everything else finished.</param>
        public GameEndGate(float hold)
        {
            if (hold < 0f)
                throw new ArgumentOutOfRangeException(nameof(hold));

            _hold = hold;
        }

        /// <summary>The playback reached the game's resolution. Its end is the one the gate waits for.</summary>
        public void SetResolved()
        {
            _isResolved = true;
            _isPlaybackEnded = false;
        }

        /// <summary>A playback ended. Counts only after the resolution was reached, so it is the playback that holds it.</summary>
        public void SetPlaybackEnded()
        {
            if (_isResolved)
                _isPlaybackEnded = true;
        }

        /// <summary>Advances the hold time. Returns true once, when the game-over screen may open.</summary>
        /// <param name="deltaTime">Seconds of game time since the last call. Zero while paused.</param>
        /// <param name="isCelebrating">Whether a celebration is still playing.</param>
        public bool Advance(float deltaTime, bool isCelebrating)
        {
            if (_isOpen)
                return false;

            if (!_isResolved || !_isPlaybackEnded || isCelebrating)
            {
                _waited = 0f;
                return false;
            }

            _waited += deltaTime;

            if (_waited < _hold)
                return false;

            _isOpen = true;
            return true;
        }
    }
}
