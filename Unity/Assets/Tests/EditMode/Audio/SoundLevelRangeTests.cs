using NUnit.Framework;

namespace Squarely.Presentation.Tests
{
    public class SoundLevelRangeTests
    {
        [TestCase(-5)]
        [TestCase(0)]
        [TestCase(100)]
        public void NoLimit_ContainsEveryLevel(int level)
        {
            Assert.IsTrue(new SoundLevelRange().Contains(level));
        }

        [Test]
        public void MinOnly_ExcludesLevelsBelow()
        {
            var range = new SoundLevelRange(2, null);

            Assert.IsFalse(range.Contains(1));
            Assert.IsTrue(range.Contains(2));
            Assert.IsTrue(range.Contains(50));
        }

        [Test]
        public void MaxOnly_ExcludesLevelsAbove()
        {
            var range = new SoundLevelRange(null, 1);

            Assert.IsTrue(range.Contains(0));
            Assert.IsTrue(range.Contains(1));
            Assert.IsFalse(range.Contains(2));
        }

        [Test]
        public void MinAndMax_ContainsOnlyBetween()
        {
            var range = new SoundLevelRange(2, 4);

            Assert.IsFalse(range.Contains(1));
            Assert.IsTrue(range.Contains(3));
            Assert.IsFalse(range.Contains(5));
        }

        [Test]
        public void MaxZero_IsAnExplicitLimit()
        {
            var range = new SoundLevelRange(null, 0);

            Assert.IsTrue(range.Contains(0));
            Assert.IsFalse(range.Contains(1));
        }
    }
}
