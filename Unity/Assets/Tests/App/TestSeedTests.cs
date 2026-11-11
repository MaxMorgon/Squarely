using System;
using NUnit.Framework;

namespace Squarely.App.Tests
{
    public sealed class TestSeedTests
    {
        private MemoryKeyValueStore _store;
        private TestSeed _testSeed;

        [SetUp]
        public void SetUp()
        {
            _store = new MemoryKeyValueStore();
            _testSeed = new TestSeed(_store, new Random(1));
        }

        #region TryParse

        [TestCase("0", 0UL)]
        [TestCase("42", 42UL)]
        [TestCase("  42 ", 42UL)]
        [TestCase("007", 7UL)]
        [TestCase("18446744073709551615", ulong.MaxValue)]
        public void TryParse_Digits_ReadsSeed(string text, ulong expected)
        {
            bool isValid = TestSeed.TryParse(text, out ulong seed);

            Assert.That(isValid, Is.True);
            Assert.That(seed, Is.EqualTo(expected));
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]
        [TestCase("-1")]
        [TestCase("+1")]
        [TestCase("1.5")]
        [TestCase("1,000")]
        [TestCase("1 2")]
        [TestCase("abc")]
        [TestCase("0x10")]
        [TestCase("18446744073709551616")]
        public void TryParse_NotASeed_ReturnsFalse(string text)
        {
            bool isValid = TestSeed.TryParse(text, out ulong _);

            Assert.That(isValid, Is.False);
        }

        [Test]
        public void Format_ThenTryParse_RoundTrips()
        {
            TestSeed.TryParse(TestSeed.Format(ulong.MaxValue), out ulong seed);

            Assert.That(seed, Is.EqualTo(ulong.MaxValue));
        }

        #endregion

        #region Persistence

        [Test]
        public void LoadOrCreate_NothingSaved_CreatesAndSavesSeed()
        {
            ulong seed = _testSeed.LoadOrCreate();

            Assert.That(seed, Is.LessThan((ulong)TestSeed.NewSeedLimit));
            Assert.That(_store.GetString(TestSeed.StoreKey, null), Is.EqualTo(TestSeed.Format(seed)));
            Assert.That(_store.SaveCount, Is.EqualTo(1));
        }

        [Test]
        public void LoadOrCreate_Twice_ReturnsSameSeed()
        {
            ulong first = _testSeed.LoadOrCreate();
            ulong second = new TestSeed(_store, new Random(2)).LoadOrCreate();

            Assert.That(second, Is.EqualTo(first));
        }

        [Test]
        public void LoadOrCreate_AfterSave_ReturnsSavedSeed()
        {
            _testSeed.Save(ulong.MaxValue);

            Assert.That(_testSeed.LoadOrCreate(), Is.EqualTo(ulong.MaxValue));
        }

        [Test]
        public void LoadOrCreate_SavedTextNotASeed_CreatesAndSavesSeed()
        {
            _store.SetString(TestSeed.StoreKey, "garbage");

            ulong seed = _testSeed.LoadOrCreate();

            Assert.That(_store.GetString(TestSeed.StoreKey, null), Is.EqualTo(TestSeed.Format(seed)));
        }

        [Test]
        public void Save_WritesToDevice()
        {
            _testSeed.Save(5);

            Assert.That(_store.GetString(TestSeed.StoreKey, null), Is.EqualTo("5"));
            Assert.That(_store.SaveCount, Is.EqualTo(1));
        }

        [Test]
        public void CreateNew_ReplacesSavedSeed()
        {
            _testSeed.Save(ulong.MaxValue);

            ulong seed = _testSeed.CreateNew();

            Assert.That(_testSeed.LoadOrCreate(), Is.EqualTo(seed));
            Assert.That(seed, Is.Not.EqualTo(ulong.MaxValue));
        }

        #endregion
    }
}
