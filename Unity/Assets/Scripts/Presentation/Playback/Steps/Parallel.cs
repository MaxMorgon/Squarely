using System;
using System.Collections.Generic;
using UnityEngine;

namespace Squarely.Presentation
{
    /// <summary>Plays its steps at the same time on the same target, and ends when the last one ends.</summary>
    [Serializable]
    public sealed class Parallel : CueStep
    {
        [SerializeReference, CueStepPicker] private List<CueStep> _steps = new List<CueStep>();

        public override async Awaitable Play(CueTarget target, PlaybackContext context)
        {
            var running = new List<Awaitable>();

            foreach (CueStep step in _steps.ToArray())
            {
                if (step != null)
                    running.Add(step.Play(target, context));
            }

            foreach (Awaitable step in running)
                await step;
        }

        public override void Snap(CueTarget target, PlaybackContext context)
        {
            foreach (CueStep step in _steps.ToArray())
            {
                if (step != null)
                    step.Snap(target, context);
            }
        }
    }
}
