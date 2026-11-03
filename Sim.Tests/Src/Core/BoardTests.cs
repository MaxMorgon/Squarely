using System;
using NUnit.Framework;

namespace Squarely.Simulation.Tests
{
    public class BoardTests
    {
        #region Placement

        [Test]
        public void NewBoard_IsEmpty()
        {
            var board = new Board(8, 8);

            for (int y = 0; y < 8; y++)
            {
                for (int x = 0; x < 8; x++)
                    Assert.That(board.IsEmpty(new GridPos(x, y)), Is.True);
            }
        }

        [Test]
        public void CanPlace_InsideEmptyBoard_IsTrue()
        {
            var board = new Board(8, 8);
            PieceShape square = PieceShape.FromPattern("square", "##/##");

            Assert.That(board.CanPlace(square, new GridPos(0, 0)), Is.True);
            Assert.That(board.CanPlace(square, new GridPos(6, 6)), Is.True);
        }

        [Test]
        public void CanPlace_PartlyOutsideBoard_IsFalse()
        {
            var board = new Board(8, 8);
            PieceShape square = PieceShape.FromPattern("square", "##/##");

            Assert.That(board.CanPlace(square, new GridPos(7, 0)), Is.False);
            Assert.That(board.CanPlace(square, new GridPos(0, 7)), Is.False);
            Assert.That(board.CanPlace(square, new GridPos(-1, 0)), Is.False);
        }

        [Test]
        public void CanPlace_OverlappingFilledCell_IsFalse()
        {
            var board = new Board(8, 8);
            board.SetCell(new GridPos(1, 1), Cell.Block);
            PieceShape square = PieceShape.FromPattern("square", "##/##");

            Assert.That(board.CanPlace(square, new GridPos(0, 0)), Is.False);
            Assert.That(board.CanPlace(square, new GridPos(2, 2)), Is.True);
        }

        [Test]
        public void CanPlace_ShapeHolesOverFilledCells_IsTrue()
        {
            var board = new Board(3, 3);
            board.SetCell(new GridPos(1, 1), Cell.Block);
            PieceShape corner = PieceShape.FromPattern("corner", "#./##");

            Assert.That(board.CanPlace(corner, new GridPos(0, 0)), Is.True);
        }

        [Test]
        public void Place_FillsShapeCells_WithPieceCellType()
        {
            var board = new Board(8, 8);
            PieceShape corner = PieceShape.FromPattern("corner", "#./##", "stone");

            board.Place(corner, new GridPos(3, 4));

            Assert.That(board.GetCell(new GridPos(3, 4)).TypeId, Is.EqualTo("stone"));
            Assert.That(board.IsEmpty(new GridPos(4, 4)), Is.False);
            Assert.That(board.IsEmpty(new GridPos(3, 5)), Is.False);
            Assert.That(board.IsEmpty(new GridPos(4, 5)), Is.True);
        }

        [Test]
        public void Place_WhereItDoesNotFit_Throws()
        {
            var board = new Board(8, 8);
            PieceShape line = PieceShape.FromPattern("line", "###");

            Assert.Throws<InvalidOperationException>(() => board.Place(line, new GridPos(6, 0)));
        }

        [Test]
        public void CanPlaceAnywhere_FindsOnlyGap()
        {
            var board = new Board(3, 3);
            TestHelpers.Fill(board, "###/#../###");

            Assert.That(board.CanPlaceAnywhere(PieceShape.FromPattern("line2", "##")), Is.True);
            Assert.That(board.CanPlaceAnywhere(PieceShape.FromPattern("dot", "#")), Is.True);
            Assert.That(board.CanPlaceAnywhere(PieceShape.FromPattern("line3", "###")), Is.False);
        }

        #endregion

        #region Clearing

        [Test]
        public void ClearCells_DuplicatesClearedOnce_InBoardOrder()
        {
            var board = new Board(3, 3);
            TestHelpers.Fill(board, ".#./.../##.");

            var cleared = board.ClearCells(new[]
            {
                new GridPos(1, 2),
                new GridPos(0, 0),
                new GridPos(1, 0),
                new GridPos(0, 0),
            });

            Assert.That(cleared, Is.EqualTo(new[] { new GridPos(0, 0), new GridPos(1, 0), new GridPos(1, 2) }));
            Assert.That(board.IsEmpty(new GridPos(1, 2)), Is.True);
        }

        [Test]
        public void ClearCells_EmptyPositions_AreNotReported()
        {
            var board = new Board(3, 3);
            board.SetCell(new GridPos(2, 2), Cell.Block);

            var cleared = board.ClearCells(new[] { new GridPos(0, 0), new GridPos(2, 2) });

            Assert.That(cleared, Is.EqualTo(new[] { new GridPos(2, 2) }));
        }

        #endregion
    }
}
