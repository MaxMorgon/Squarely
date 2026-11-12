using NUnit.Framework;

namespace Squarely.Presentation.Tests
{
    public class SoundDecibelsTests
    {
        [Test]
        public void Toggle_OnIsZeroDb_OffIsMinus80Db()
        {
            Assert.AreEqual(0f, SoundDecibels.ForToggle(true));
            Assert.AreEqual(-80f, SoundDecibels.ForToggle(false));
        }

        [Test]
        public void FromLinear_FullVolumeIsZeroDb_HalfIsAboutMinus6Db()
        {
            Assert.AreEqual(0f, SoundDecibels.FromLinear(1f), 0.0001f);
            Assert.AreEqual(-6.02f, SoundDecibels.FromLinear(0.5f), 0.01f);
        }

        [Test]
        public void FromLinear_Zero_IsOff()
        {
            Assert.AreEqual(SoundDecibels.Off, SoundDecibels.FromLinear(0f));
        }
    }
}
