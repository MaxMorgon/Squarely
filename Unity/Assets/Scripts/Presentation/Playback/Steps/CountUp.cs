using System;
using System.Collections.Generic;
using UnityEngine;

namespace Squarely.Presentation
{
    /// <summary>Counts the target's number from its start value to its end value. Can play a tick each time the shown number changes, at most once per cooldown of the tick cue.</summary>
    [Serializable]
    public sealed class CountUp : CueStep
    {
        private const float PreviewFrameSeconds = 1f / 60f;

        [SerializeField] private float _duration = 0.4f;

        [Tooltip("Progress of the count over the animation (time 0 to 1, value 0 to 1).")]
        [SerializeField] private AnimationCurve _progress = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        [Tooltip("Plays each time the shown number changes. Empty: no tick.")]
        [SerializeField] private SoundCue _tickCue;

        public override float PlannedSeconds => _duration;

        private float TickSeconds => _tickCue != null ? _tickCue.Cooldown : 0f;

        public override async Awaitable Play(CueTarget target, PlaybackContext context)
        {
            if (!target.HasCount)
                return;

            float elapsed = 0f;
            var gate = new CountTickGate(target.CountFrom, TickSeconds);
            SetCount(target, 0f);

            while (elapsed < _duration)
            {
                if (!await context.NextFrameAsync(target))
                    return;

                elapsed += context.DeltaTime;
                int shown = SetCount(target, Progress(elapsed, _duration));

                if (_tickCue != null && context.Views.Sound != null && gate.Advance(shown, elapsed))
                    context.Views.Sound.Play(_tickCue, 0);
            }

            target.SetCount(target.CountTo);
        }

        public override void Snap(CueTarget target, PlaybackContext context)
        {
            target.SetCount(target.CountTo);
        }

        public override void CollectSounds(float startSeconds, SoundLevelInput input, List<ScheduledSound> sounds)
        {
            if (_tickCue == null)
                return;

            var gate = new CountTickGate(input.CountFrom, TickSeconds);
            float elapsed = 0f;

            while (elapsed < _duration)
            {
                elapsed = Mathf.Min(elapsed + PreviewFrameSeconds, _duration);
                int shown = ValueAt(input.CountFrom, input.Count, Progress(elapsed, _duration));

                if (gate.Advance(shown, elapsed))
                    sounds.Add(new ScheduledSound(startSeconds + elapsed, _tickCue, 0));
            }
        }

        private int SetCount(CueTarget target, float t)
        {
            int value = ValueAt(target.CountFrom, target.CountTo, t);
            target.SetCount(value);
            return value;
        }

        private int ValueAt(int from, int to, float t)
        {
            return Mathf.RoundToInt(Mathf.LerpUnclamped(from, to, _progress.Evaluate(t)));
        }
    }
}
