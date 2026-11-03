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
        public void Place_FillsShapeCells()
        {
            var board = new Board(8, 8);
            PieceShape corner = PieceShape.FromPattern("corner", "#./##");

            board.Place(corner, new GridPos(3, 4), Cell.Block);

            Assert.That(board.IsEmpty(new GridPos(3, 4)), Is.False);
            Assert.That(board.IsEmpty(new GridPos(4, 4)), Is.False);
            Assert.That(board.IsEmpty(new GridPos(3, 5)), Is.False);
            Assert.That(board.IsEmpty(new GridPos(4, 5)), Is.True);
        }

        [Test]
        public void Place_WhereItDoesNotFit_Throws()
        {
            var board = new Board(8, 8);
            PieceShape line = PieceShape.FromPattern("line", "###");

            Assert.Throws<InvalidOperationException>(() => board.Place(line, new GridPos(6, 0), Cell.Block));
        }

        [Test]
        public void CanPlaceAnywhere_FindsOnlyGap()
        {
            var board = new Board(3, 3);
            TestHelpers.Fill(board, "###/#../###");
            PieceShape line2 = PieceShape.FromPattern("line2", "##");
            PieceShape dot = PieceShape.FromPattern("dot", "#");

            Assert.That(board.CanPlaceAnywhere(line2), Is.True);
            Assert.That(board.CanPlaceAnywhere(dot), Is.True);
            Assert.That(board.CanPlaceAnywhere(PieceShape.FromPattern("line3", "###")), Is.False);
        }

        #endregion

        #region Line clearing

        [Test]
        public void FindFullLines_NoFullLines_ReturnsEmpty()
        {
            var board = new Board(3, 3);
            TestHelpers.Fill(board, "##./##./...");

            Assert.That(board.FindFullLines().IsEmpty, Is.True);
        }

        [Test]
        public void FindFullLines_FullRow_IsFound()
        {
            var board = new Board(3, 3);
            TestHelpers.Fill(board, "###");

            BoardLines lines = board.FindFullLines();

            Assert.That(lines.Rows, Is.EqualTo(new[] { 0 }));
            Assert.That(lines.Columns, Is.Empty);
        }

        [Test]
        public void FindFullLines_FullColumn_IsFound()
        {
            var board = new Board(3, 3);
            TestHelpers.Fill(board, "..#/..#/..#");

            BoardLines lines = board.FindFullLines();

            Assert.That(lines.Rows, Is.Empty);
            Assert.That(lines.Columns, Is.EqualTo(new[] { 2 }));
        }

        [Test]
        public void ClearLines_RowAndColumn_SharedCellClearedOnce()
        {
            var board = new Board(3, 3);
            // Row 0 and column 0 full, plus one extra cell that must survive.
            TestHelpers.Fill(board, "#../##./###");

            BoardLines lines = board.FindFullLines();
            var cleared = board.ClearLines(lines);

            Assert.That(lines.Rows, Is.EqualTo(new[] { 0 }));
            Assert.That(lines.Columns, Is.EqualTo(new[] { 0 }));
            Assert.That(cleared.Count, Is.EqualTo(5));
            Assert.That(board.IsEmpty(new GridPos(1, 1)), Is.False);
            Assert.That(board.IsEmpty(new GridPos(0, 0)), Is.True);
            Assert.That(board.IsEmpty(new GridPos(2, 0)), Is.True);
            Assert.That(board.IsEmpty(new GridPos(0, 2)), Is.True);
        }

        [Test]
        public void ClearLines_MultipleRows_AllCleared()
        {
            // 3 wide, 4 tall: rows 0 and 1 full, one survivor on row 3 (no full column).
            var board = new Board(3, 4);
            TestHelpers.Fill(board, "#../.../###/###");

            BoardLines lines = board.FindFullLines();
            var cleared = board.ClearLines(lines);

            Assert.That(lines.Rows, Is.EqualTo(new[] { 0, 1 }));
            Assert.That(lines.Columns, Is.Empty);
            Assert.That(cleared.Count, Is.EqualTo(6));
            Assert.That(board.IsEmpty(new GridPos(0, 3)), Is.False);
        }

        #endregion
    }
}
