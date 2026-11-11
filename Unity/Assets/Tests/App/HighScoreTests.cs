using NUnit.Framework;

namespace Squarely.App.Tests
{
    public sealed class HighScoreTests
    {
        private FakeKeyValueStore _store;
        private HighScore _highScore;

        [SetUp]
        public void SetUp()
        {
            _store = new FakeKeyValueStore();
            _highScore = new HighScore(_store);
        }

        [Test]
        public void Best_BeforeFirstGame_IsNull()
        {
            Assert.That(_highScore.Best, Is.Null);
        }

        [Test]
        public void Record_FirstGame_SetsItAndIsNew()
        {
            bool isNew = _highScore.Record(120);

            Assert.That(isNew, Is.True);
            Assert.That(_highScore.Best, Is.EqualTo(120));
        }

        [Test]
        public void Record_FirstGameWithZero_SetsItAndIsNew()
        {
            bool isNew = _highScore.Record(0);

            Assert.That(isNew, Is.True);
            Assert.That(_highScore.Best, Is.EqualTo(0));
        }

        [Test]
        public void Record_LowerScore_KeepsBestAndIsNotNew()
        {
            _highScore.Record(120);

            bool isNew = _highScore.Record(80);

            Assert.That(isNew, Is.False);
            Assert.That(_highScore.Best, Is.EqualTo(120));
        }

        [Test]
        public void Record_EqualScore_IsNotNew()
        {
            _highScore.Record(120);

            bool isNew = _highScore.Record(120);

            Assert.That(isNew, Is.False);
            Assert.That(_highScore.Best, Is.EqualTo(120));
        }

        [Test]
        public void Record_HigherScore_ReplacesBestAndIsNew()
        {
            _highScore.Record(120);

            bool isNew = _highScore.Record(200);

            Assert.That(isNew, Is.True);
            Assert.That(_highScore.Best, Is.EqualTo(200));
        }

        [Test]
        public void Record_NewHighScore_IsSavedToTheStore()
        {
            _highScore.Record(120);

            Assert.That(_store.GetInt(HighScore.StoreKey, -1), Is.EqualTo(120));
            Assert.That(_store.SaveCount, Is.EqualTo(1));
        }

        [Test]
        public void Record_LowerScore_DoesNotWriteTheStore()
        {
            _highScore.Record(120);

            _highScore.Record(80);

            Assert.That(_store.SaveCount, Is.EqualTo(1));
        }

        [Test]
        public void Best_NewInstanceOnSameStore_ReadsSavedScore()
        {
            _highScore.Record(120);

            var reloaded = new HighScore(_store);

            Assert.That(reloaded.Best, Is.EqualTo(120));
            Assert.That(reloaded.Record(100), Is.False);
        }
    }
}
