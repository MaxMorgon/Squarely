using System;
using System.Collections.Generic;
using NUnit.Framework;
using Squarely.Simulation;

namespace Squarely.Presentation.Tests
{
    public class PiecePlacedHandlerTests
    {
        // Board after placing a two-cell piece at (0, 1) and (1, 1).
        private static readonly GridPos[] Filled =
        {
            new GridPos(0, 0), new GridPos(1, 0), new GridPos(2, 0), new GridPos(0, 1), new GridPos(1, 1),
        };

        private static readonly GridPos[] Placed = { new GridPos(0, 1), new GridPos(1, 1) };

        [Test]
        public void ClearedCells_UnitesEveryClear()
        {
            var events = new ISimEvent[]
            {
                new ScoreChangedEvent(10, 10),
                Cleared(new GridPos(1, 0), new GridPos(2, 0)),
                Cleared(new GridPos(2, 0), new GridPos(0, 1)),
            };

            Assert.That(PiecePlacedHandler.ClearedCells(events),
                Is.EquivalentTo(new[] { new GridPos(1, 0), new GridPos(2, 0), new GridPos(0, 1) }));
        }

        [Test]
        public void ClearedCells_NoClear_IsEmpty()
        {
            var events = new ISimEvent[] { new ScoreChangedEvent(10, 10) };

            Assert.That(PiecePlacedHandler.ClearedCells(events), Is.Empty);
        }

        [Test]
        public void WaveCells_NoClear_LeavesOutPlacedCells_InOrder()
        {
            List<GridPos> wave = PiecePlacedHandler.WaveCells(Filled, Placed, new HashSet<GridPos>());

            Assert.That(wave, Is.EqualTo(new[] { new GridPos(0, 0), new GridPos(1, 0), new GridPos(2, 0) }));
        }

        [Test]
        public void WaveCells_Clear_LeavesOutClearedCells_KeepsPlacedCellsThatStay()
        {
            var cleared = new HashSet<GridPos> { new GridPos(1, 0), new GridPos(2, 0), new GridPos(0, 1) };

            List<GridPos> wave = PiecePlacedHandler.WaveCells(Filled, Placed, cleared);

            Assert.That(wave, Is.EqualTo(new[] { new GridPos(0, 0), new GridPos(1, 1) }));
        }

        [Test]
        public void WaveCells_OnlyPlacedCells_NoClear_IsEmpty()
        {
            List<GridPos> wave = PiecePlacedHandler.WaveCells(Placed, Placed, new HashSet<GridPos>());

            Assert.That(wave, Is.Empty);
        }

        private static PatternsClearedEvent Cleared(params GridPos[] cells)
        {
            return new PatternsClearedEvent(Array.Empty<ClearedMatch>(), cells);
        }
    }
}
