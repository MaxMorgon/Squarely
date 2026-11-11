using System;
using System.Collections.Generic;
using System.Threading;
using Squarely.Simulation;
using UnityEngine;

namespace Squarely.Presentation
{
    /// <summary>
    /// Shared by the handlers of one command's playback: the views, results earlier handlers leave for later ones,
    /// the cue runs started so far, and the playback clock.
    /// </summary>
    public sealed class PlaybackContext
    {
        private readonly List<CueRun> _runs = new List<CueRun>();
        private readonly Queue<CueRun> _blockingRuns = new Queue<CueRun>();
        private readonly Func<float> _speed;
        private readonly CancellationToken _stopToken;

        public PlaybackViews Views { get; }

        /// <summary>The board cells of the piece placed by this command, empty until the placement is handled.</summary>
        public IReadOnlyList<GridPos> PlacedCells { get; internal set; } = Array.Empty<GridPos>();

        /// <summary>World position of the last cleared match's center, or of the placed piece if nothing cleared yet.</summary>
        public Vector3 LastEffectPosition { get; internal set; }

        /// <summary>The playback was snapped: everything already applied its end state.</summary>
        public bool IsSnapped { get; private set; }

        /// <summary>Steps must stop touching views: the playback was snapped, or its views are being destroyed.</summary>
        public bool IsStopped => IsSnapped || _stopToken.IsCancellationRequested;

        /// <summary>This frame's playback time step, in seconds, scaled by the playback speed.</summary>
        public float DeltaTime => Time.deltaTime * _speed();

        internal PlaybackContext(PlaybackViews views, Func<float> speed, CancellationToken stopToken)
        {
            Views = views;
            _speed = speed;
            _stopToken = stopToken;
        }

        #region Cues

        /// <summary>Plays a cue sequence on a target. The steps are read from the asset now, their values as each step plays.</summary>
        /// <param name="onEnd">Called once when the run ends, played out or snapped, e.g. to release detached views.</param>
        public CueRun Play(CueSequence sequence, CueTarget target, Action onEnd = null)
        {
            if (sequence == null)
                return Play(Array.Empty<CueStep>(), false, target, onEnd);

            return Play(sequence.Steps, sequence.WaitForEnd, target, onEnd);
        }

        /// <summary>Plays steps on a target. If <paramref name="waitForEnd"/> is set, the next event waits for them to finish.</summary>
        /// <param name="onEnd">Called once when the run ends, played out or snapped, e.g. to release detached views.</param>
        public CueRun Play(IReadOnlyList<CueStep> steps, bool waitForEnd, CueTarget target, Action onEnd = null)
        {
            var run = new CueRun(steps, target, this, onEnd);
            _runs.Add(run);

            if (IsSnapped)
            {
                run.Snap();
                return run;
            }

            run.Start();

            if (waitForEnd)
                _blockingRuns.Enqueue(run);

            return run;
        }

        #endregion

        #region Waiting

        /// <summary>Waits one frame. Returns false if the playback stopped meanwhile: the caller then returns without touching views.</summary>
        public async Awaitable<bool> NextFrameAsync()
        {
            await Awaitable.NextFrameAsync();
            return !IsStopped;
        }

        internal async Awaitable WaitForBlockingRunsAsync()
        {
            while (_blockingRuns.Count > 0 && !IsStopped)
                await _blockingRuns.Dequeue().WaitAsync();
        }

        internal async Awaitable WaitForAllRunsAsync()
        {
            for (int i = 0; i < _runs.Count && !IsStopped; i++)
                await _runs[i].WaitAsync();
        }

        #endregion

        internal void Snap()
        {
            IsSnapped = true;

            // Snapping can run handlers that start new runs; those snap as they start.
            CueRun[] runs = _runs.ToArray();

            foreach (CueRun run in runs)
                run.Snap();
        }
    }
}
