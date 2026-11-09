using System;
using System.Collections.Generic;

namespace Squarely.App
{
    /// <summary>
    /// Holds the current app state and moves between states: the current state exits, then the next one enters.
    /// A change requested while a transition is running (from inside Enter or Exit) is queued
    /// and runs after that transition completes, so states always exit and enter in pairs.
    /// </summary>
    public sealed class AppStateMachine
    {
        private readonly Queue<IAppState> _pending = new Queue<IAppState>();
        private bool _isTransitioning;

        /// <summary>The state last entered, or null before the first change.</summary>
        public IAppState Current { get; private set; }

        public void ChangeState(IAppState next)
        {
            if (next == null)
                throw new ArgumentNullException(nameof(next));

            _pending.Enqueue(next);

            if (_isTransitioning)
                return;

            _isTransitioning = true;

            try
            {
                while (_pending.Count > 0)
                    Transition(_pending.Dequeue());
            }
            finally
            {
                _isTransitioning = false;
                _pending.Clear();
            }
        }

        private void Transition(IAppState next)
        {
            if (Current != null)
                Current.Exit();

            Current = next;
            Current.Enter();
        }
    }
}
