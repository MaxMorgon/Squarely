using NUnit.Framework;

namespace Squarely.Presentation.Tests
{
    public class SoundGateTests
    {
        [Test]
        public void FirstPlay_IsAllowed()
        {
            Assert.IsTrue(SoundGate.CanPlay(0.0, double.NegativeInfinity, 5f, 0, 1));
        }

        [Test]
        public void InsideCooldown_IsSkipped()
        {
            Assert.IsFalse(SoundGate.CanPlay(1.0, 0.9, 0.2f, 0, 4));
        }

        [Test]
        public void AfterCooldown_IsAllowed()
        {
            Assert.IsTrue(SoundGate.CanPlay(1.0, 0.7, 0.2f, 0, 4));
        }

        [Test]
        public void AtVoiceLimit_IsSkipped()
        {
            Assert.IsFalse(SoundGate.CanPlay(1.0, 0.0, 0f, 4, 4));
        }

        [Test]
        public void BelowVoiceLimit_IsAllowed()
        {
            Assert.IsTrue(SoundGate.CanPlay(1.0, 0.0, 0f, 3, 4));
        }

        [Test]
        public void VoiceLimitBelowOne_CountsAsOne()
        {
            Assert.IsTrue(SoundGate.CanPlay(1.0, 0.0, 0f, 0, 0));
            Assert.IsFalse(SoundGate.CanPlay(1.0, 0.0, 0f, 1, 0));
        }
    }
}
