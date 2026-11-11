using NUnit.Framework;
using Squarely.Simulation;
using UnityEngine;

namespace Squarely.Presentation.Tests
{
    public class BoardSpaceTests
    {
        private const float Tolerance = 0.0001f;

        [Test]
        public void CellCenter_BoardIsCenteredOnOrigin()
        {
            var boardSpace = new BoardSpace(8, 8, 1f);

            AssertNear(boardSpace.CellCenter(new GridPos(0, 0)), new Vector2(-3.5f, -3.5f));
            AssertNear(boardSpace.CellCenter(new GridPos(7, 7)), new Vector2(3.5f, 3.5f));
        }

        [Test]
        public void CellCenter_ScalesWithCellSize_OnNonSquareBoard()
        {
            var boardSpace = new BoardSpace(3, 2, 2f);

            AssertNear(boardSpace.CellCenter(new GridPos(0, 0)), new Vector2(-2f, -1f));
            AssertNear(boardSpace.CellCenter(new GridPos(2, 1)), new Vector2(2f, 1f));
        }

        [Test]
        public void NearestCell_OfEveryCellCenter_IsThatCell()
        {
            var boardSpace = new BoardSpace(5, 4, 1.5f);

            for (int y = 0; y < boardSpace.Height; y++)
            {
                for (int x = 0; x < boardSpace.Width; x++)
                {
                    var pos = new GridPos(x, y);
                    Assert.That(boardSpace.NearestCell(boardSpace.CellCenter(pos)), Is.EqualTo(pos));
                }
            }
        }

        [Test]
        public void NearestCell_SwitchesAtTheCellEdge()
        {
            var boardSpace = new BoardSpace(8, 8, 1f);
            Vector2 center = boardSpace.CellCenter(new GridPos(0, 0));

            Assert.That(boardSpace.NearestCell(center + new Vector2(0.49f, 0.49f)), Is.EqualTo(new GridPos(0, 0)));
            Assert.That(boardSpace.NearestCell(center + new Vector2(0.51f, 0f)), Is.EqualTo(new GridPos(1, 0)));
            Assert.That(boardSpace.NearestCell(center + new Vector2(0f, 0.51f)), Is.EqualTo(new GridPos(0, 1)));
        }

        [Test]
        public void NearestCell_OutsideBoard_ReturnsCellOutsideBoard()
        {
            var boardSpace = new BoardSpace(8, 8, 1f);

            GridPos pos = boardSpace.NearestCell(new Vector2(-4.8f, 4.8f));

            Assert.That(pos, Is.EqualTo(new GridPos(-1, 8)));
        }

        [Test]
        public void CellCoordinates_AreCellUnits_WithCentersOnWholeNumbers()
        {
            var boardSpace = new BoardSpace(8, 8, 2f);
            Vector2 center = boardSpace.CellCenter(new GridPos(3, 5));

            AssertNear(boardSpace.CellCoordinates(center), new Vector2(3f, 5f));
            AssertNear(boardSpace.CellCoordinates(center + new Vector2(1f, -0.5f)), new Vector2(3.5f, 4.75f));
        }

        [Test]
        public void LocalBounds_CoverEveryCell_CenteredOnOrigin()
        {
            var boardSpace = new BoardSpace(3, 2, 2f);
            Rect bounds = boardSpace.LocalBounds;

            AssertNear(bounds.min, new Vector2(-3f, -2f));
            AssertNear(bounds.max, new Vector2(3f, 2f));
        }

        private static void AssertNear(Vector2 actual, Vector2 expected)
        {
            Assert.That(actual.x, Is.EqualTo(expected.x).Within(Tolerance));
            Assert.That(actual.y, Is.EqualTo(expected.y).Within(Tolerance));
        }
    }
}
