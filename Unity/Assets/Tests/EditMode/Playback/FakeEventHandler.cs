using System.Collections.Generic;
using Squarely.Simulation;
using UnityEngine;

namespace Squarely.Presentation.Tests
{
    /// <summary>Plays a <see cref="FakeEvent"/>'s steps and counts handling and run ends on the event.</summary>
    public sealed class FakeEventHandler : SimEventHandler<FakeEvent>
    {
        protected override void Handle(FakeEvent simEvent, PlaybackContext context)
        {
            simEvent.HandleCount++;
            context.SnapRunsOn(simEvent.Cells);
            context.Play(simEvent.Steps, simEvent.WaitForEnd, TargetFor(simEvent), () => simEvent.RunEndCount++);
        }

        private static CueTarget TargetFor(FakeEvent simEvent)
        {
            if (simEvent.Cells.Count == 0)
                return CueTarget.ForTransform(null, Vector3.zero);

            var positions = new List<GridPos>();

            for (int i = 0; i < simEvent.Cells.Count; i++)
                positions.Add(new GridPos(i, 0));

            return CueTarget.ForCells(simEvent.Cells, positions, Vector3.zero);
        }
    }
}
