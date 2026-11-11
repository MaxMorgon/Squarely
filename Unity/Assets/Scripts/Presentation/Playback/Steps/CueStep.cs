using System;
using UnityEngine;

namespace Squarely.Presentation
{
    /// <summary>
    /// One step of a <see cref="CueSequence"/>. A new step type is one small class with serialized settings.
    /// Steps live in a shared asset, so they keep no state between calls: everything a run needs is in the target and context.
    /// </summary>
    [Serializable]
    public abstract class CueStep
    {
        /// <summary>
        /// Plays the step and completes when it ends. Reads its settings as it plays, so asset edits apply on the next play.
        /// It waits for frames with <see cref="PlaybackContext.NextFrameAsync"/> and returns without touching anything once that returns false.
        /// </summary>
        public abstract Awaitable Play(CueTarget target, PlaybackContext context);

        /// <summary>
        /// Applies the step's end state at once. Called on the running step and on steps that have not started,
        /// and may be called on a step that already played. Transient effects do nothing here.
        /// </summary>
        public abstract void Snap(CueTarget target, PlaybackContext context);

        /// <summary>An awaitable that has already completed, for steps that end as soon as they start.</summary>
        protected static Awaitable Completed()
        {
            var source = new AwaitableCompletionSource();
            source.SetResult();
            return source.Awaitable;
        }

        /// <summary>Progress from 0 to 1 of an animation of the given duration, <paramref name="time"/> seconds after it started.</summary>
        protected static float Progress(float time, float duration)
        {
            if (duration <= 0f)
                return time >= 0f ? 1f : 0f;

            return Mathf.Clamp01(time / duration);
        }
    }
}
