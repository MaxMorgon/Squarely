using System;
using NUnit.Framework;
using Squarely.Simulation;
using UnityEngine;

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
            float[] delays = CellOrdering.Delays(CellOrder.Listed, Row, null, null, Stagger);

            AssertDelays(delays, 0f, 0.1f, 0.2f, 0.3f);
        }

        [Test]
        public void FromCenter_BothEndsOfALineStartTogether()
        {
            float[] delays = CellOrdering.Delays(CellOrder.FromCenter, Row, null, null, Stagger);

            AssertDelays(delays, 0.1f, 0f, 0f, 0.1f);
        }

        [Test]
        public void FromPlacement_NearestToPlacedCellStartsFirst()
        {
            GridPos[] placed = { new GridPos(3, 1) };

            float[] delays = CellOrdering.Delays(CellOrder.FromPlacement, Row, placed, null, Stagger);

            Assert.That(delays[3], Is.EqualTo(0f).Within(Tolerance));
            Assert.That(delays[2], Is.GreaterThan(delays[3]));
            Assert.That(delays[1], Is.GreaterThan(delays[2]));
            Assert.That(delays[0], Is.GreaterThan(delays[1]));
        }

        [Test]
        public void FromPlacement_NothingPlaced_FallsBackToListed()
        {
            float[] delays = CellOrdering.Delays(CellOrder.FromPlacement, Row, Array.Empty<GridPos>(), null, Stagger);

            AssertDelays(delays, 0f, 0.1f, 0.2f, 0.3f);
        }

        [Test]
        public void FromPointer_DelaysFollowContinuousDistanceToDropPoint()
        {
            // Distances from (0.5, -1): sqrt(1.25), sqrt(1.25), sqrt(3.25), sqrt(7.25).
            float[] delays = CellOrdering.Delays(CellOrder.FromPointer, Row, null, new Vector2(0.5f, -1f), Stagger);

            float nearest = Mathf.Sqrt(1.25f);
            AssertDelays(delays, 0f, 0f, (Mathf.Sqrt(3.25f) - nearest) * Stagger, (Mathf.Sqrt(7.25f) - nearest) * Stagger);
        }

        [Test]
        public void FromPointer_NoDropPoint_StartsFromPlacedPieceCenter()
        {
            GridPos[] placed = { new GridPos(2, 1), new GridPos(3, 1) };

            float[] delays = CellOrdering.Delays(CellOrder.FromPointer, Row, placed, null, Stagger);

            float[] expected = CellOrdering.Delays(CellOrder.FromPointer, Row, null, new Vector2(2.5f, 1f), Stagger);
            AssertDelays(delays, expected);
        }

        [Test]
        public void FromPointer_NoDropPointAndNothingPlaced_StartsFromTargetCenter()
        {
            float[] delays = CellOrdering.Delays(CellOrder.FromPointer, Row, null, null, Stagger);

            AssertDelays(delays, 0.1f, 0f, 0f, 0.1f);
        }

        [Test]
        public void NoCells_NoDelays()
        {
            float[] delays = CellOrdering.Delays(CellOrder.FromCenter, Array.Empty<GridPos>(), null, null, Stagger);

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
