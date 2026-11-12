using NUnit.Framework;

namespace Squarely.App.Tests
{
    public sealed class StartInTestTests
    {
        private MemoryKeyValueStore _store;
        private StartInTest _startInTest;

        [SetUp]
        public void SetUp()
        {
            _store = new MemoryKeyValueStore();
            _startInTest = new StartInTest(_store);
        }

        [Test]
        public void IsOn_NeverSet_IsOff()
        {
            Assert.That(_startInTest.IsOn, Is.False);
        }

        [Test]
        public void Set_On_IsOnAndSaved()
        {
            _startInTest.Set(true);

            Assert.That(_startInTest.IsOn, Is.True);
            Assert.That(_store.SaveCount, Is.EqualTo(1));
        }

        [Test]
        public void Set_OnThenOff_IsOff()
        {
            _startInTest.Set(true);

            _startInTest.Set(false);

            Assert.That(_startInTest.IsOn, Is.False);
        }

        [Test]
        public void IsOn_NewInstanceOnSameStore_ReadsSavedOption()
        {
            _startInTest.Set(true);

            var reloaded = new StartInTest(_store);

            Assert.That(reloaded.IsOn, Is.True);
        }
    }
}
