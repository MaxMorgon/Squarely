#if UNITY_EDITOR || SQUARELY_DEV_TOOLS
using System;

namespace Squarely.App
{
    /// <summary>
    /// Decides when a replay watch runs its next command. In auto-play, the next command is due once the
    /// playback of the previous one has ended and the delay has passed. A step runs the next command at any time.
    /// Counts the commands run. Time is the caller's: scale it by the playback speed before you pass it.
    /// </summary>
    public sealed class ReplayStepper
    {
        private readonly float _delay;
        private float _waited;
        private bool _isStopped;

        /// <summary>How many commands the replay has.</summary>
        public int Total { get; }

        /// <summary>How many commands ran.</summary>
        public int Executed { get; private set; }

        /// <summary>Whether auto-play is off.</summary>
        public bool IsPaused { get; private set; }

        /// <summary>Whether a next command can run: some are left and the replay did not stop.</summary>
        public bool HasNext => !_isStopped && Executed < Total;

        /// <param name="delay">Seconds between the end of a command's playback and the next command in auto-play.</param>
        public ReplayStepper(int total, float delay)
        {
            if (total < 0)
                throw new ArgumentOutOfRangeException(nameof(total));

            Total = total;
            _delay = delay;
        }

        public void SetPaused(bool isPaused)
        {
            IsPaused = isPaused;
            _waited = 0f;
        }

        /// <summary>
        /// Advances auto-play time. Returns true when the next command is due: the caller then runs it
        /// and calls <see cref="MarkExecuted"/>. Never due while paused or while the previous playback runs.
        /// </summary>
        public bool Tick(float deltaTime, bool isPlaybackRunning)
        {
            if (IsPaused || !HasNext)
                return false;

            if (isPlaybackRunning)
            {
                _waited = 0f;
                return false;
            }

            _waited += deltaTime;
            return _waited >= _delay;
        }

        /// <summary>Counts one command as run and restarts the delay.</summary>
        public void MarkExecuted()
        {
            Executed++;
            _waited = 0f;
        }

        /// <summary>Ends the replay early, for a command that failed. No next command is due after it.</summary>
        public void Stop()
        {
            _isStopped = true;
        }
    }
}
#endif
