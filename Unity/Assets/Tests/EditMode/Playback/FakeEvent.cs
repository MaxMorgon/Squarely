using System.Collections.Generic;
using Squarely.Simulation;

namespace Squarely.Presentation.Tests
{
    /// <summary>An event whose handler plays the given steps, and that records how its run ended.</summary>
    public sealed class FakeEvent : ISimEvent
    {
        public IReadOnlyList<CueStep> Steps { get; }

        public bool WaitForEnd { get; }

        /// <summary>Cells the handler takes over (snapping runs on them) and plays on. Empty: it plays on a transform target.</summary>
        public IReadOnlyList<CellView> Cells { get; set; } = new CellView[0];

        public int HandleCount { get; set; }

        public int RunEndCount { get; set; }

        public FakeEvent(bool waitForEnd, params CueStep[] steps)
        {
            WaitForEnd = waitForEnd;
            Steps = steps;
        }
    }
}
