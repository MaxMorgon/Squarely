using System;
using UnityEngine;

namespace Squarely.Presentation
{
    /// <summary>Counts the target's number from its start value to its end value.</summary>
    [Serializable]
    public sealed class CountUp : CueStep
    {
        [SerializeField] private float _duration = 0.4f;

        [Tooltip("Progress of the count over the animation (time 0 to 1, value 0 to 1).")]
        [SerializeField] private AnimationCurve _progress = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        public override async Awaitable Play(CueTarget target, PlaybackContext context)
        {
            if (!target.HasCount)
                return;

            float elapsed = 0f;
            SetCount(target, 0f);

            while (elapsed < _duration)
            {
                if (!await context.NextFrameAsync())
                    return;

                elapsed += context.DeltaTime;
                SetCount(target, Progress(elapsed, _duration));
            }

            target.SetCount(target.CountTo);
        }

        public override void Snap(CueTarget target, PlaybackContext context)
        {
            target.SetCount(target.CountTo);
        }

        private void SetCount(CueTarget target, float t)
        {
            float value = Mathf.LerpUnclamped(target.CountFrom, target.CountTo, _progress.Evaluate(t));
            target.SetCount(Mathf.RoundToInt(value));
        }
    }
}
