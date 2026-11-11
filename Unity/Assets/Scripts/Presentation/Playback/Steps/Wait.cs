using System;
using UnityEngine;

namespace Squarely.Presentation
{
    /// <summary>Waits before the next step.</summary>
    [Serializable]
    public sealed class Wait : CueStep
    {
        [SerializeField] private float _seconds = 0.1f;

        public override async Awaitable Play(CueTarget target, PlaybackContext context)
        {
            float elapsed = 0f;

            while (elapsed < _seconds)
            {
                if (!await context.NextFrameAsync(target))
                    return;

                elapsed += context.DeltaTime;
            }
        }

        public override void Snap(CueTarget target, PlaybackContext context)
        {
        }
    }
}
