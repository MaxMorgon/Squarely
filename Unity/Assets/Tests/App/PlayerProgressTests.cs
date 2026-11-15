using NUnit.Framework;

namespace Squarely.App.Tests
{
    public sealed class PlayerProgressTests
    {
        private MemoryKeyValueStore _store;
        private HighScore _highScore;
        private PlayerProgress _progress;

        [SetUp]
        public void SetUp()
        {
            _store = new MemoryKeyValueStore();
            _highScore = new HighScore(_store);
            _progress = new PlayerProgress(_highScore);
        }

        [Test]
        public void Reset_ClearsTheHighScore()
        {
            _highScore.Record(120);

            _progress.Reset();

            Assert.That(_highScore.Best, Is.Null);
        }

        [Test]
        public void Reset_SavesAtOnce()
        {
            _highScore.Record(120);
            int savesBefore = _store.SaveCount;

            _progress.Reset();

            Assert.That(_store.SaveCount, Is.EqualTo(savesBefore + 1));
        }

        [Test]
        public void Reset_AfterReset_NextGameIsANewHighScore()
        {
            _highScore.Record(120);
            _progress.Reset();

            Assert.That(_highScore.Record(5), Is.True);
            Assert.That(_highScore.Best, Is.EqualTo(5));
        }

        [Test]
        public void Reset_KeepsSettingsAndOtherValues()
        {
            var devTools = new DevToolsPreference(_store);
            devTools.Set(false);
            _store.SetInt("SomeSetting", 7);
            _highScore.Record(120);

            _progress.Reset();

            Assert.That(devTools.IsOn, Is.False);
            Assert.That(_store.GetInt("SomeSetting", 0), Is.EqualTo(7));
        }

        [Test]
        public void Reset_WithNoHighScore_DoesNothingBad()
        {
            _progress.Reset();

            Assert.That(_highScore.Best, Is.Null);
        }
    }
}
