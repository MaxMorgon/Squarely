using System;
using NUnit.Framework;
using Squarely.Simulation;

namespace Squarely.Presentation.Tests
{
    public class CellOrderingTests
    {
        private const float Stagger = 0.1f;
        private const float Tolerance = 0.0001f;

        private static readonly GridPos[] Row =
        {
            new GridPos(0, 0), new GridPos(1, 0), new GridPos(2, 0), new GridPos(3, 0),
        };

        [Test]
        public void Listed_DelaysFollowListOrder()
        {
            float[] delays = CellOrdering.Delays(CellOrder.Listed, Row, null, Stagger);

            AssertDelays(delays, 0f, 0.1f, 0.2f, 0.3f);
        }

        [Test]
        public void FromCenter_BothEndsOfALineStartTogether()
        {
            float[] delays = CellOrdering.Delays(CellOrder.FromCenter, Row, null, Stagger);

            AssertDelays(delays, 0.1f, 0f, 0f, 0.1f);
        }

        [Test]
        public void FromPlacement_NearestToPlacedCellStartsFirst()
        {
            GridPos[] placed = { new GridPos(3, 1) };

            float[] delays = CellOrdering.Delays(CellOrder.FromPlacement, Row, placed, Stagger);

            Assert.That(delays[3], Is.EqualTo(0f).Within(Tolerance));
            Assert.That(delays[2], Is.GreaterThan(delays[3]));
            Assert.That(delays[1], Is.GreaterThan(delays[2]));
            Assert.That(delays[0], Is.GreaterThan(delays[1]));
        }

        [Test]
        public void FromPlacement_NothingPlaced_FallsBackToListed()
        {
            float[] delays = CellOrdering.Delays(CellOrder.FromPlacement, Row, Array.Empty<GridPos>(), Stagger);

            AssertDelays(delays, 0f, 0.1f, 0.2f, 0.3f);
        }

        [Test]
        public void NoCells_NoDelays()
        {
            float[] delays = CellOrdering.Delays(CellOrder.FromCenter, Array.Empty<GridPos>(), null, Stagger);

            Assert.That(delays, Is.Empty);
        }

        private static void AssertDelays(float[] actual, params float[] expected)
        {
            Assert.That(actual.Length, Is.EqualTo(expected.Length));

            for (int i = 0; i < expected.Length; i++)
                Assert.That(actual[i], Is.EqualTo(expected[i]).Within(Tolerance), $"cell {i}");
        }
    }
}
