using System;
using NUnit.Framework;
using Squarely.Simulation;

namespace Squarely.Presentation.Tests
{
    public class SlotGridTests
    {
        private static readonly GridPos Pos = new GridPos(1, 2);

        [Test]
        public void Get_EmptySlot_IsNull()
        {
            var grid = new SlotGrid<string>(3, 3);

            Assert.That(grid.Get(Pos), Is.Null);
        }

        [Test]
        public void Set_ThenGet_ReturnsItem()
        {
            var grid = new SlotGrid<string>(3, 3);

            grid.Set(Pos, "a");

            Assert.That(grid.Get(Pos), Is.EqualTo("a"));
            Assert.That(grid.Get(new GridPos(2, 1)), Is.Null);
        }

        [Test]
        public void Set_OccupiedSlot_Throws()
        {
            var grid = new SlotGrid<string>(3, 3);
            grid.Set(Pos, "a");

            Assert.Throws<InvalidOperationException>(() => grid.Set(Pos, "b"));
        }

        [Test]
        public void Detach_ReturnsItem_AndEmptiesSlot()
        {
            var grid = new SlotGrid<string>(3, 3);
            grid.Set(Pos, "a");

            Assert.That(grid.Detach(Pos), Is.EqualTo("a"));
            Assert.That(grid.Get(Pos), Is.Null);
        }

        [Test]
        public void Detach_ThenSet_SlotTakesNewItem()
        {
            var grid = new SlotGrid<string>(3, 3);
            grid.Set(Pos, "a");

            grid.Detach(Pos);
            grid.Set(Pos, "b");

            Assert.That(grid.Get(Pos), Is.EqualTo("b"));
        }

        [Test]
        public void Detach_EmptySlot_ReturnsNull()
        {
            var grid = new SlotGrid<string>(3, 3);

            Assert.That(grid.Detach(Pos), Is.Null);
        }

        [Test]
        public void OutsideGrid_Throws()
        {
            var grid = new SlotGrid<string>(3, 3);

            Assert.Throws<ArgumentOutOfRangeException>(() => grid.Get(new GridPos(3, 0)));
            Assert.Throws<ArgumentOutOfRangeException>(() => grid.Set(new GridPos(0, -1), "a"));
        }
    }
}
