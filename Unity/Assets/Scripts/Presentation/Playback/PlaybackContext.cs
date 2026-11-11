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

        /// <summary>
        /// Where the pointer dropped the piece of this command, in board cell coordinates (<see cref="BoardSpace.CellCoordinates"/>),
        /// or null if no pointer placed it.
        /// </summary>
        public Vector2? DropPoint { get; }

        /// <summary>World position of the last cleared match's center, or of the placed piece if nothing cleared yet.</summary>
        public Vector3 LastEffectPosition { get; internal set; }

        /// <summary>The playback was snapped: everything already applied its end state.</summary>
        public bool IsSnapped { get; private set; }

        /// <summary>Steps must stop touching views: the playback was snapped, or its views are being destroyed.</summary>
        public bool IsStopped => IsSnapped || _stopToken.IsCancellationRequested;

        /// <summary>This frame's playback time step, in seconds, scaled by the playback speed.</summary>
        public float DeltaTime => Time.deltaTime * _speed();

        internal PlaybackContext(PlaybackViews views, Vector2? dropPoint, Func<float> speed, CancellationToken stopToken)
        {
            Views = views;
            DropPoint = dropPoint;
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
        /// <exception cref="InvalidOperationException">The target is already played by another run.</exception>
        public CueRun Play(IReadOnlyList<CueStep> steps, bool waitForEnd, CueTarget target, Action onEnd = null)
        {
            if (target.IsPlayed)
                throw new InvalidOperationException("A cue target is played by one run only.");

            target.IsPlayed = true;
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

        /// <summary>
        /// Snaps every run of this playback that is still playing on any of these cells. Handlers call it before they
        /// detach or release cell views, so no run animates a view after it has left the board or hand.
        /// </summary>
        public void SnapRunsOn(IReadOnlyList<CellView> cells)
        {
            if (cells.Count == 0)
                return;

            // Snapping a run can release views, but never starts a run.
            foreach (CueRun run in _runs.ToArray())
            {
                if (!run.IsEnded && SharesAny(run.Target.Cells, cells))
                    run.Snap();
            }
        }

        private static bool SharesAny(IReadOnlyList<CellView> first, IReadOnlyList<CellView> second)
        {
            foreach (CellView cell in first)
            {
                foreach (CellView other in second)
                {
                    if (cell == other)
                        return true;
                }
            }

            return false;
        }

        #endregion

        #region Waiting

        /// <summary>
        /// Waits one frame. Returns false if the playback stopped or the run on <paramref name="target"/> was snapped meanwhile:
        /// the caller then returns without touching views.
        /// </summary>
        public async Awaitable<bool> NextFrameAsync(CueTarget target)
        {
            await Awaitable.NextFrameAsync();
            return !IsStopped && !target.IsSnapped;
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
