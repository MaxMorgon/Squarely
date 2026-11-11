using UnityEngine;

namespace Squarely.Presentation.Tests
{
    /// <summary>Plays a <see cref="FakeEvent"/>'s steps and counts handling and run ends on the event.</summary>
    public sealed class FakeEventHandler : SimEventHandler<FakeEvent>
    {
        protected override void Handle(FakeEvent simEvent, PlaybackContext context)
        {
            simEvent.HandleCount++;
            CueTarget target = CueTarget.ForTransform(null, Vector3.zero);
            context.Play(simEvent.Steps, simEvent.WaitForEnd, target, () => simEvent.RunEndCount++);
        }
    }
}
