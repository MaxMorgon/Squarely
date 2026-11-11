using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Squarely.Presentation
{
    /// <summary>
    /// One playing cue sequence on one target: plays its steps in order, or snaps the running and unstarted ones.
    /// Ends once, either way.
    /// </summary>
    public sealed class CueRun
    {
        private readonly CueStep[] _steps;
        private readonly CueTarget _target;
        private readonly PlaybackContext _context;
        private readonly Action _onEnd;

        private int _next;
        private Awaitable _task;
        private bool _awaited;

        public bool IsEnded { get; private set; }

        internal CueTarget Target => _target;

        internal CueRun(IReadOnlyList<CueStep> steps, CueTarget target, PlaybackContext context, Action onEnd)
        {
            _steps = steps.ToArray();
            _target = target;
            _context = context;
            _onEnd = onEnd;
        }

        internal void Start()
        {
            _task = PlayAsync();
        }

        /// <summary>Applies the end state of the running step and of every step not started yet, then ends the run.</summary>
        internal void Snap()
        {
            if (IsEnded)
                return;

            _target.IsSnapped = true;

            for (int i = _next; i < _steps.Length; i++)
            {
                if (_steps[i] != null)
                    _steps[i].Snap(_target, _context);
            }

            _next = _steps.Length;
            End();
        }

        /// <summary>Waits for the run to play out. Each run is waited for at most once.</summary>
        internal async Awaitable WaitAsync()
        {
            if (_task == null || _awaited)
                return;

            _awaited = true;
            await _task;
        }

        private async Awaitable PlayAsync()
        {
            while (_next < _steps.Length)
            {
                if (_context.IsStopped || IsEnded)
                    return;

                CueStep step = _steps[_next];

                if (step != null)
                    await step.Play(_target, _context);

                // Snapped while the step played: Snap already applied the end state and ended the run.
                if (_context.IsStopped || IsEnded)
                    return;

                _next++;
            }

            End();
        }

        private void End()
        {
            if (IsEnded)
                return;

            IsEnded = true;
            _onEnd?.Invoke();
        }
    }
}
