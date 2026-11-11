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
        public void FromPointer_StartsFromPlacedCellNearestDropPoint()
        {
            GridPos[] placed = { new GridPos(2, 1), new GridPos(3, 1) };

            // Starts from (3, 1): distances 1, sqrt(2), sqrt(5), sqrt(10) from the end of the row back.
            float[] delays = CellOrdering.Delays(CellOrder.FromPointer, Row, placed, new Vector2(3.4f, 1.2f), Stagger);

            AssertDelays(delays, 0.3f, 0.2f, 0.1f, 0f);
        }

        [Test]
        public void FromPointer_NothingPlaced_StartsFromTargetCellNearestDropPoint()
        {
            // (0, 0) and (1, 0) are equally near: the first listed wins.
            float[] delays = CellOrdering.Delays(CellOrder.FromPointer, Row, null, new Vector2(0.5f, -1f), Stagger);

            AssertDelays(delays, 0f, 0.1f, 0.2f, 0.3f);
        }

        [Test]
        public void FromPointer_FarDropPoint_FirstCellStillStartsAtZero()
        {
            float[] delays = CellOrdering.Delays(CellOrder.FromPointer, Row, null, new Vector2(-10f, 0f), Stagger);

            AssertDelays(delays, 0f, 0.1f, 0.2f, 0.3f);
        }

        [Test]
        public void FromPointer_NoDropPoint_StartsFromPlacedPieceCenter()
        {
            GridPos[] placed = { new GridPos(2, 1), new GridPos(3, 1) };

            // From (2.5, 1): (2, 0) and (3, 0) first, then (1, 0), then (0, 0).
            float[] delays = CellOrdering.Delays(CellOrder.FromPointer, Row, placed, null, Stagger);

            AssertDelays(delays, 0.2f, 0.1f, 0f, 0f);
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
