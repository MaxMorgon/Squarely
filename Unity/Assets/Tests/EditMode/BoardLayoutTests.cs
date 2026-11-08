using NUnit.Framework;
using Squarely.Simulation;
using UnityEngine;

namespace Squarely.Presentation.Tests
{
    public class BoardLayoutTests
    {
        private const float Tolerance = 0.0001f;

        [Test]
        public void CellCenter_BoardIsCenteredOnOrigin()
        {
            var layout = new BoardLayout(8, 8, 1f);

            AssertNear(layout.CellCenter(new GridPos(0, 0)), new Vector2(-3.5f, -3.5f));
            AssertNear(layout.CellCenter(new GridPos(7, 7)), new Vector2(3.5f, 3.5f));
        }

        [Test]
        public void CellCenter_ScalesWithCellSize_OnNonSquareBoard()
        {
            var layout = new BoardLayout(3, 2, 2f);

            AssertNear(layout.CellCenter(new GridPos(0, 0)), new Vector2(-2f, -1f));
            AssertNear(layout.CellCenter(new GridPos(2, 1)), new Vector2(2f, 1f));
        }

        [Test]
        public void NearestCell_OfEveryCellCenter_IsThatCell()
        {
            var layout = new BoardLayout(5, 4, 1.5f);

            for (int y = 0; y < layout.Height; y++)
            {
                for (int x = 0; x < layout.Width; x++)
                {
                    var pos = new GridPos(x, y);
                    Assert.That(layout.NearestCell(layout.CellCenter(pos)), Is.EqualTo(pos));
                }
            }
        }

        [Test]
        public void NearestCell_SwitchesAtTheCellEdge()
        {
            var layout = new BoardLayout(8, 8, 1f);
            Vector2 center = layout.CellCenter(new GridPos(0, 0));

            Assert.That(layout.NearestCell(center + new Vector2(0.49f, 0.49f)), Is.EqualTo(new GridPos(0, 0)));
            Assert.That(layout.NearestCell(center + new Vector2(0.51f, 0f)), Is.EqualTo(new GridPos(1, 0)));
            Assert.That(layout.NearestCell(center + new Vector2(0f, 0.51f)), Is.EqualTo(new GridPos(0, 1)));
        }

        [Test]
        public void NearestCell_OutsideBoard_ReturnsCellOutsideBoard()
        {
            var layout = new BoardLayout(8, 8, 1f);

            GridPos pos = layout.NearestCell(new Vector2(-4.8f, 4.8f));

            Assert.That(pos, Is.EqualTo(new GridPos(-1, 8)));
            Assert.That(layout.Contains(pos), Is.False);
        }

        private static void AssertNear(Vector2 actual, Vector2 expected)
        {
            Assert.That(actual.x, Is.EqualTo(expected.x).Within(Tolerance));
            Assert.That(actual.y, Is.EqualTo(expected.y).Within(Tolerance));
        }
    }
}
