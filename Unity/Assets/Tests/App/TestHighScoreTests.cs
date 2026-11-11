using NUnit.Framework;

namespace Squarely.App.Tests
{
    public sealed class TestHighScoreTests
    {
        [Test]
        public void Create_BestIsTheGivenValue()
        {
            HighScore highScore = TestHighScore.Create(250);

            Assert.That(highScore.Best, Is.EqualTo(250));
        }

        [Test]
        public void Create_Zero_HasABestScoreOfZero()
        {
            HighScore highScore = TestHighScore.Create(0);

            Assert.That(highScore.Best, Is.EqualTo(0));
        }

        [Test]
        public void Record_HigherScore_IsNewButNotSavedOnTheDevice()
        {
            var device = new MemoryKeyValueStore();
            var deviceHighScore = new HighScore(device);
            deviceHighScore.Record(100);

            HighScore testHighScore = TestHighScore.Create(0);
            bool isNew = testHighScore.Record(500);

            Assert.That(isNew, Is.True);
            Assert.That(testHighScore.Best, Is.EqualTo(500));
            Assert.That(deviceHighScore.Best, Is.EqualTo(100));
            Assert.That(device.SaveCount, Is.EqualTo(1));
        }

        [TestCase("0", 0)]
        [TestCase("42", 42)]
        [TestCase("  42 ", 42)]
        [TestCase("2147483647", int.MaxValue)]
        public void TryParse_Digits_ReadsHighScore(string text, int expected)
        {
            bool isValid = TestHighScore.TryParse(text, out int highScore);

            Assert.That(isValid, Is.True);
            Assert.That(highScore, Is.EqualTo(expected));
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("-1")]
        [TestCase("1.5")]
        [TestCase("2147483648")]
        public void TryParse_NotAHighScore_IsFalse(string text)
        {
            Assert.That(TestHighScore.TryParse(text, out _), Is.False);
        }

        [Test]
        public void Format_ReadsBackWithTryParse()
        {
            TestHighScore.TryParse(TestHighScore.Format(1234), out int highScore);

            Assert.That(highScore, Is.EqualTo(1234));
        }
    }
}
