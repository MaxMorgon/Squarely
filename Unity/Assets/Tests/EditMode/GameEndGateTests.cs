using NUnit.Framework;

namespace Squarely.Presentation.Tests
{
    public class GameEndGateTests
    {
        [Test]
        public void Advance_BeforeResolved_StaysClosed()
        {
            var gate = new GameEndGate(0.5f);
            gate.SetPlaybackEnded();

            Assert.IsFalse(gate.Advance(10f, false));
        }

        [Test]
        public void Advance_ResolvedButPlaybackRunning_StaysClosed()
        {
            var gate = new GameEndGate(0.5f);
            gate.SetResolved();

            Assert.IsFalse(gate.Advance(10f, false));
        }

        [Test]
        public void SetPlaybackEnded_BeforeResolved_IsIgnored()
        {
            var gate = new GameEndGate(0f);
            gate.SetPlaybackEnded();
            gate.SetResolved();

            Assert.IsFalse(gate.Advance(1f, false));
        }

        [Test]
        public void Advance_AfterPlaybackEnd_WaitsForHold()
        {
            var gate = new GameEndGate(0.5f);
            gate.SetResolved();
            gate.SetPlaybackEnded();

            Assert.IsFalse(gate.Advance(0.3f, false));
            Assert.IsTrue(gate.Advance(0.3f, false));
        }

        [Test]
        public void Advance_ZeroHold_OpensAtOnce()
        {
            var gate = new GameEndGate(0f);
            gate.SetResolved();
            gate.SetPlaybackEnded();

            Assert.IsTrue(gate.Advance(0f, false));
        }

        [Test]
        public void Advance_Paused_DoesNotCountHold()
        {
            var gate = new GameEndGate(0.5f);
            gate.SetResolved();
            gate.SetPlaybackEnded();

            Assert.IsFalse(gate.Advance(0f, false));
            Assert.IsFalse(gate.Advance(0f, false));
            Assert.IsTrue(gate.Advance(0.5f, false));
        }

        [Test]
        public void Advance_Celebrating_RestartsHoldAfterwards()
        {
            var gate = new GameEndGate(0.5f);
            gate.SetResolved();
            gate.SetPlaybackEnded();

            Assert.IsFalse(gate.Advance(0.4f, false));
            Assert.IsFalse(gate.Advance(0.1f, true));
            Assert.IsFalse(gate.Advance(0.4f, false));
            Assert.IsTrue(gate.Advance(0.1f, false));
        }

        [Test]
        public void Advance_AfterOpening_ReturnsTrueOnlyOnce()
        {
            var gate = new GameEndGate(0f);
            gate.SetResolved();
            gate.SetPlaybackEnded();

            Assert.IsTrue(gate.Advance(1f, false));
            Assert.IsFalse(gate.Advance(1f, false));

            gate.SetResolved();
            gate.SetPlaybackEnded();
            Assert.IsFalse(gate.Advance(1f, false));
        }
    }
}
