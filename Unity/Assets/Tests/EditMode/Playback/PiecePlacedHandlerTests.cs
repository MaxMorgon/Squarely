using System;
using NUnit.Framework;
using Squarely.Simulation;

namespace Squarely.Presentation.Tests
{
    public class PiecePlacedHandlerTests
    {
        private static readonly GridPos[] Filled =
        {
            new GridPos(0, 0), new GridPos(1, 0), new GridPos(2, 0), new GridPos(0, 1),
        };

        [Test]
        public void WaveCells_NoClear_KeepsEveryFilledCellInOrder()
        {
            var events = new ISimEvent[] { new ScoreChangedEvent(10, 10) };

            Assert.That(PiecePlacedHandler.WaveCells(Filled, events), Is.EqualTo(Filled));
        }

        [Test]
        public void WaveCells_LeavesOutClearedCells()
        {
            var events = new ISimEvent[]
            {
                Cleared(new GridPos(1, 0), new GridPos(2, 0)),
                Cleared(new GridPos(0, 1)),
            };

            Assert.That(PiecePlacedHandler.WaveCells(Filled, events), Is.EqualTo(new[] { new GridPos(0, 0) }));
        }

        [Test]
        public void WaveCells_EmptyBoard_IsEmpty()
        {
            Assert.That(PiecePlacedHandler.WaveCells(Array.Empty<GridPos>(), Array.Empty<ISimEvent>()), Is.Empty);
        }

        private static PatternsClearedEvent Cleared(params GridPos[] cells)
        {
            return new PatternsClearedEvent(Array.Empty<ClearedMatch>(), cells);
        }
    }
}
