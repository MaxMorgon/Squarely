using System;

namespace Squarely.App
{
    /// <summary>
    /// Holds the current app state and moves between states: the current state exits, then the next one enters.
    /// Changing state while a transition is running (from inside Enter or Exit) is an error.
    /// </summary>
    public sealed class AppStateMachine
    {
        private bool _isTransitioning;

        /// <summary>The state last entered, or null before the first change.</summary>
        public IAppState Current { get; private set; }

        /// <summary>Runs one frame of the current state, if it is an <see cref="ITickableState"/>. Does nothing during a transition.</summary>
        public void Tick(float deltaTime)
        {
            if (_isTransitioning)
                return;

            if (Current is ITickableState tickable)
                tickable.Tick(deltaTime);
        }

        /// <exception cref="InvalidOperationException">Called from inside a state's Enter or Exit.</exception>
        public void ChangeState(IAppState next)
        {
            if (next == null)
                throw new ArgumentNullException(nameof(next));

            if (_isTransitioning)
                throw new InvalidOperationException("Cannot change state while a transition is running (from Enter or Exit).");

            _isTransitioning = true;

            try
            {
                if (Current != null)
                    Current.Exit();

                Current = next;
                Current.Enter();
            }
            finally
            {
                _isTransitioning = false;
            }
        }
    }
}
