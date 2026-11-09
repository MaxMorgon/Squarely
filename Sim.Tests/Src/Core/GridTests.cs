using System;
using NUnit.Framework;

namespace Squarely.Simulation.Tests
{
    public class GridTests
    {
        private static readonly GridPos Pos = new GridPos(1, 2);

        [Test]
        public void NewGrid_HoldsDefaultValues()
        {
            var grid = new Grid<string>(3, 3);

            Assert.That(grid[Pos], Is.Null);
        }

        [Test]
        public void Set_ThenGet_ReturnsValue_OnlyAtThatPosition()
        {
            var grid = new Grid<string>(3, 3);

            grid[Pos] = "a";

            Assert.That(grid[Pos], Is.EqualTo("a"));
            Assert.That(grid[new GridPos(2, 1)], Is.Null);
        }

        [Test]
        public void NonSquareGrid_StoresEveryPositionSeparately()
        {
            var grid = new Grid<int>(4, 2);

            for (int y = 0; y < 2; y++)
            {
                for (int x = 0; x < 4; x++)
                    grid[new GridPos(x, y)] = y * 10 + x;
            }

            for (int y = 0; y < 2; y++)
            {
                for (int x = 0; x < 4; x++)
                    Assert.That(grid[new GridPos(x, y)], Is.EqualTo(y * 10 + x));
            }
        }

        [Test]
        public void IsWithinBounds_IsTrueOnlyInsideTheGrid()
        {
            var grid = new Grid<int>(4, 2);

            Assert.That(grid.IsWithinBounds(new GridPos(0, 0)), Is.True);
            Assert.That(grid.IsWithinBounds(new GridPos(3, 1)), Is.True);
            Assert.That(grid.IsWithinBounds(new GridPos(4, 0)), Is.False);
            Assert.That(grid.IsWithinBounds(new GridPos(0, 2)), Is.False);
            Assert.That(grid.IsWithinBounds(new GridPos(-1, 0)), Is.False);
        }

        [Test]
        public void OutsideGrid_Throws()
        {
            var grid = new Grid<int>(3, 3);

            Assert.Throws<ArgumentOutOfRangeException>(() => _ = grid[new GridPos(3, 0)]);
            Assert.Throws<ArgumentOutOfRangeException>(() => grid[new GridPos(0, -1)] = 1);
        }

        [Test]
        public void NonPositiveSize_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new Grid<int>(0, 3));
            Assert.Throws<ArgumentOutOfRangeException>(() => new Grid<int>(3, -1));
        }

        [Test]
        public void Clone_IsIndependentOfOriginal()
        {
            var grid = new Grid<int>(3, 3);
            grid[Pos] = 1;

            Grid<int> copy = grid.Clone();
            copy[Pos] = 2;

            Assert.That(grid[Pos], Is.EqualTo(1));
            Assert.That(copy[Pos], Is.EqualTo(2));
            Assert.That(copy.Width, Is.EqualTo(3));
            Assert.That(copy.Height, Is.EqualTo(3));
        }
    }
}
