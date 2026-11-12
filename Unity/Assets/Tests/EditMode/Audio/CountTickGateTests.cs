using NUnit.Framework;

namespace Squarely.Presentation.Tests
{
    public class CountTickGateTests
    {
        [Test]
        public void Advance_SameNumber_NoTick()
        {
            var gate = new CountTickGate(5, 0.05f);

            Assert.IsFalse(gate.Advance(5, 0.1f));
        }

        [Test]
        public void Advance_NumberChanges_Ticks()
        {
            var gate = new CountTickGate(0, 0.05f);

            Assert.IsTrue(gate.Advance(1, 0f));
        }

        [Test]
        public void Advance_ChangeSoonerThanTheMinimum_NoTick()
        {
            var gate = new CountTickGate(0, 0.05f);
            gate.Advance(1, 0f);

            Assert.IsFalse(gate.Advance(2, 0.02f));
        }

        [Test]
        public void Advance_ChangeAfterTheMinimum_Ticks()
        {
            var gate = new CountTickGate(0, 0.05f);
            gate.Advance(1, 0f);
            gate.Advance(2, 0.02f);

            Assert.IsTrue(gate.Advance(3, 0.05f));
        }

        [Test]
        public void Advance_SkippedChangeDoesNotTickLater()
        {
            var gate = new CountTickGate(0, 0.05f);
            gate.Advance(1, 0f);
            gate.Advance(2, 0.02f);

            Assert.IsFalse(gate.Advance(2, 0.1f));
        }
    }
}
