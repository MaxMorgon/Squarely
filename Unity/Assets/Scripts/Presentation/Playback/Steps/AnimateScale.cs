using System;
using UnityEngine;

namespace Squarely.Presentation
{
    /// <summary>Animates the scale of the target's transform, e.g. a punch on a HUD label.</summary>
    [Serializable]
    public sealed class AnimateScale : CueStep
    {
        [SerializeField] private float _duration = 0.25f;

        [Tooltip("Scale over the animation (time 0 to 1). End at 1 to leave the transform at its normal size.")]
        [SerializeField] private AnimationCurve _scale = AnimationCurve.Constant(0f, 1f, 1f);

        public override async Awaitable Play(CueTarget target, PlaybackContext context)
        {
            Transform transform = target.Transform;

            if (transform == null)
                return;

            float elapsed = 0f;
            SetScale(transform, 0f);

            while (elapsed < _duration)
            {
                if (!await context.NextFrameAsync())
                    return;

                elapsed += context.DeltaTime;
                SetScale(transform, Progress(elapsed, _duration));
            }
        }

        public override void Snap(CueTarget target, PlaybackContext context)
        {
            if (target.Transform != null)
                SetScale(target.Transform, 1f);
        }

        private void SetScale(Transform transform, float t)
        {
            transform.localScale = Vector3.one * _scale.Evaluate(t);
        }
    }
}
