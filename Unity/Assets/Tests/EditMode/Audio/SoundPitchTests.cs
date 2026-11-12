using NUnit.Framework;

namespace Squarely.Presentation.Tests
{
    public class SoundPitchTests
    {
        private const float Tolerance = 0.0001f;

        [Test]
        public void Variation_RandomZeroGivesMin_RandomOneGivesMax()
        {
            Assert.AreEqual(0.9f, SoundPitch.Calculate(0.9f, 1.1f, 0f, 0f, 0, 8), Tolerance);
            Assert.AreEqual(1.1f, SoundPitch.Calculate(0.9f, 1.1f, 1f, 0f, 0, 8), Tolerance);
            Assert.AreEqual(1f, SoundPitch.Calculate(0.9f, 1.1f, 0.5f, 0f, 0, 8), Tolerance);
        }

        [Test]
        public void Variation_RandomOutsideRange_IsClamped()
        {
            Assert.AreEqual(1.1f, SoundPitch.Calculate(0.9f, 1.1f, 5f, 0f, 0, 8), Tolerance);
            Assert.AreEqual(0.9f, SoundPitch.Calculate(0.9f, 1.1f, -5f, 0f, 0, 8), Tolerance);
        }

        [Test]
        public void Step_TwelveSemitonesAtLevelOne_DoublesPitch()
        {
            Assert.AreEqual(2f, SoundPitch.Calculate(1f, 1f, 0.5f, 12f, 1, 8), Tolerance);
        }

        [Test]
        public void Step_LevelZero_IsBasePitch()
        {
            Assert.AreEqual(1f, SoundPitch.Calculate(1f, 1f, 0.5f, 3f, 0, 8), Tolerance);
        }

        [Test]
        public void Step_AboveMaxLevel_UsesMaxLevel()
        {
            float atMax = SoundPitch.Calculate(1f, 1f, 0.5f, 2f, 4, 4);
            Assert.AreEqual(atMax, SoundPitch.Calculate(1f, 1f, 0.5f, 2f, 99, 4), Tolerance);
        }

        [Test]
        public void Step_NegativeLevel_IsBasePitch()
        {
            Assert.AreEqual(1f, SoundPitch.Calculate(1f, 1f, 0.5f, 2f, -3, 4), Tolerance);
        }

        [Test]
        public void Step_AndVariation_Multiply()
        {
            Assert.AreEqual(2.2f, SoundPitch.Calculate(1.1f, 1.1f, 0f, 12f, 1, 8), Tolerance);
        }
    }
}
