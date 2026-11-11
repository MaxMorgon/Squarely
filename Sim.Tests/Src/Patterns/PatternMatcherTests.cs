using System.Collections.Generic;
using NUnit.Framework;

namespace Squarely.Simulation.Tests
{
    public class PatternMatcherTests
    {
        #region Rows and columns

        [Test]
        public void NoFullLines_NoMatches()
        {
            var board = new Board(3, 3);
            TestHelpers.Fill(board, "##./##./...");

            Assert.That(PatternMatcher.FindMatches(board, LinePatterns(3, 3)), Is.Empty);
        }

        [Test]
        public void FullRow_Matches()
        {
            var board = new Board(3, 3);
            TestHelpers.Fill(board, "###");

            List<PatternMatch> matches = PatternMatcher.FindMatches(board, LinePatterns(3, 3));

            Assert.That(matches.Count, Is.EqualTo(1));
            Assert.That(matches[0].Pattern.Id, Is.EqualTo("row_0"));
            Assert.That(matches[0].Positions.Count, Is.EqualTo(3));
        }

        [Test]
        public void FullColumn_Matches()
        {
            var board = new Board(3, 3);
            TestHelpers.Fill(board, "..#/..#/..#");

            List<PatternMatch> matches = PatternMatcher.FindMatches(board, LinePatterns(3, 3));

            Assert.That(matches.Count, Is.EqualTo(1));
            Assert.That(matches[0].Pattern.Id, Is.EqualTo("column_2"));
        }

        [Test]
        public void RowAndColumn_BothMatch_RowsFirst()
        {
            var board = new Board(3, 3);
            TestHelpers.Fill(board, "#../##./###");

            List<PatternMatch> matches = PatternMatcher.FindMatches(board, LinePatterns(3, 3));

            Assert.That(matches.Count, Is.EqualTo(2));
            Assert.That(matches[0].Pattern.Id, Is.EqualTo("row_0"));
            Assert.That(matches[1].Pattern.Id, Is.EqualTo("column_0"));
        }

        #endregion

        #region Custom patterns

        [Test]
        public void UnanchoredPattern_MatchesAnywhere()
        {
            var board = new Board(5, 5);
            board.SetCell(new GridPos(3, 2), Cell.Block);
            board.SetCell(new GridPos(4, 2), Cell.Block);
            BoardPattern pair = BoardPattern.FromText("pair", "##", 5, null);

            List<PatternMatch> matches = PatternMatcher.FindMatches(board, new[] { pair });

            Assert.That(matches.Count, Is.EqualTo(1));
            Assert.That(matches[0].Origin, Is.EqualTo(new GridPos(3, 2)));
        }

        [Test]
        public void AnchoredPattern_OnlyMatchesAtAnchor()
        {
            var board = new Board(5, 5);
            board.SetCell(new GridPos(3, 2), Cell.Block);
            board.SetCell(new GridPos(4, 2), Cell.Block);

            BoardPattern atAnchor = BoardPattern.FromText("pair", "##", 5, new GridPos(3, 2));
            BoardPattern elsewhere = BoardPattern.FromText("pair", "##", 5, new GridPos(0, 0));

            Assert.That(PatternMatcher.FindMatches(board, new[] { atAnchor }).Count, Is.EqualTo(1));
            Assert.That(PatternMatcher.FindMatches(board, new[] { elsewhere }), Is.Empty);
        }

        [Test]
        public void MustBeEmptyCells_BlockMatchWhenFilled()
        {
            // A smiley-like pattern: eyes filled, gap between them must stay empty.
            var board = new Board(3, 1);
            BoardPattern eyes = BoardPattern.FromText("eyes", "#o#", 5, null);

            board.SetCell(new GridPos(0, 0), Cell.Block);
            board.SetCell(new GridPos(2, 0), Cell.Block);
            Assert.That(PatternMatcher.FindMatches(board, new[] { eyes }).Count, Is.EqualTo(1));

            board.SetCell(new GridPos(1, 0), Cell.Block);
            Assert.That(PatternMatcher.FindMatches(board, new[] { eyes }), Is.Empty);
        }

        [Test]
        public void CellTypeCondition_MatchesOnlyThatType()
        {
            var board = new Board(2, 1);
            var stoneOnly = new BoardPattern(
                "stone",
                new[] { new PatternCell(new GridPos(0, 0), new CellTypeCondition("stone")) },
                5,
                null);

            board.SetCell(new GridPos(0, 0), Cell.Block);
            board.SetCell(new GridPos(1, 0), new Cell("stone"));

            List<PatternMatch> matches = PatternMatcher.FindMatches(board, new[] { stoneOnly });

            Assert.That(matches.Count, Is.EqualTo(1));
            Assert.That(matches[0].Origin, Is.EqualTo(new GridPos(1, 0)));
        }

        #endregion

        #region AnyMatch

        [Test]
        public void AnyMatch_NoFullLines_False()
        {
            var board = new Board(3, 3);
            TestHelpers.Fill(board, "##./##./...");

            Assert.That(PatternMatcher.AnyMatch(board, LinePatterns(3, 3)), Is.False);
        }

        [Test]
        public void AnyMatch_FullColumn_True()
        {
            var board = new Board(3, 3);
            TestHelpers.Fill(board, "..#/..#/..#");

            Assert.That(PatternMatcher.AnyMatch(board, LinePatterns(3, 3)), Is.True);
        }

        [Test]
        public void AnyMatch_AnchoredPattern_OnlyAtAnchor()
        {
            var board = new Board(5, 5);
            board.SetCell(new GridPos(3, 2), Cell.Block);
            board.SetCell(new GridPos(4, 2), Cell.Block);

            BoardPattern atAnchor = BoardPattern.FromText("pair", "##", 5, new GridPos(3, 2));
            BoardPattern elsewhere = BoardPattern.FromText("pair", "##", 5, new GridPos(0, 0));

            Assert.That(PatternMatcher.AnyMatch(board, new[] { atAnchor }), Is.True);
            Assert.That(PatternMatcher.AnyMatch(board, new[] { elsewhere }), Is.False);
        }

        #endregion

        private static List<BoardPattern> LinePatterns(int width, int height)
        {
            return new ClearingConfig().BuildPatterns(width, height);
        }
    }
}
