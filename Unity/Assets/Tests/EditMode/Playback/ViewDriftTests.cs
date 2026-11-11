using NUnit.Framework;
using Squarely.Simulation;

namespace Squarely.Presentation.Tests
{
    public class ViewDriftTests
    {
        private Sim _sim;

        [SetUp]
        public void SetUp()
        {
            _sim = new Sim(DefaultSimConfig.Create(), 1);
        }

        [Test]
        public void Find_ViewsMatchSim_IsEmpty()
        {
            ViewDrift drift = Find(ShownFromSim());

            Assert.That(drift.IsEmpty, Is.True);
        }

        [Test]
        public void Find_ExtraCellShown_ListsThatCell()
        {
            ShownState shown = ShownFromSim();
            shown.Cells[new GridPos(2, 3)] = Cell.BlockType;

            ViewDrift drift = Find(shown);

            Assert.That(drift.IsEmpty, Is.False);
            Assert.That(drift.Cells, Is.EqualTo(new[] { new GridPos(2, 3) }));
            Assert.That(drift.ToString(), Does.Contain(new GridPos(2, 3).ToString()));
        }

        [Test]
        public void Find_HandSlotShowsOtherPiece_ListsThatSlot()
        {
            ShownState shown = ShownFromSim();
            shown.Hand[1] = null;

            ViewDrift drift = Find(shown);

            Assert.That(drift.HandSlots, Is.EqualTo(new[] { 1 }));
            Assert.That(drift.Cells, Is.Empty);
        }

        [Test]
        public void Find_ScoreAndComboDiffer_ReportsBoth()
        {
            ShownState matching = ShownFromSim();
            var shown = new ShownState(matching.Cells, matching.Hand, _sim.Score + 10, _sim.Combo + 1);

            ViewDrift drift = Find(shown);

            Assert.That(drift.ScoreDiffers, Is.True);
            Assert.That(drift.ComboDiffers, Is.True);
        }

        private ViewDrift Find(ShownState shown)
        {
            return ViewDrift.Find(shown, _sim.Board, _sim.Hand, _sim.Score, _sim.Combo);
        }

        private ShownState ShownFromSim()
        {
            var cells = new Grid<string>(_sim.Board.Width, _sim.Board.Height);

            for (int y = 0; y < cells.Height; y++)
            {
                for (int x = 0; x < cells.Width; x++)
                {
                    var pos = new GridPos(x, y);
                    cells[pos] = _sim.Board.GetCell(pos).TypeId;
                }
            }

            var hand = new PieceInstance[_sim.Hand.Size];

            for (int slot = 0; slot < hand.Length; slot++)
                hand[slot] = _sim.Hand.GetSlot(slot);

            return new ShownState(cells, hand, _sim.Score, _sim.Combo);
        }
    }
}
