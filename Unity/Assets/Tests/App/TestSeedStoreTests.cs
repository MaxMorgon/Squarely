using System;
using NUnit.Framework;

namespace Squarely.App.Tests
{
    public sealed class TestSeedStoreTests
    {
        private FakeKeyValueStore _store;
        private TestSeedStore _seeds;

        [SetUp]
        public void SetUp()
        {
            _store = new FakeKeyValueStore();
            _seeds = new TestSeedStore(_store, new Random(1));
        }

        #region TryParse

        [TestCase("0", 0UL)]
        [TestCase("42", 42UL)]
        [TestCase("  42 ", 42UL)]
        [TestCase("007", 7UL)]
        [TestCase("18446744073709551615", ulong.MaxValue)]
        public void TryParse_Digits_ReadsSeed(string text, ulong expected)
        {
            bool isValid = TestSeedStore.TryParse(text, out ulong seed);

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
            bool isValid = TestSeedStore.TryParse(text, out ulong _);

            Assert.That(isValid, Is.False);
        }

        [Test]
        public void Format_ThenTryParse_RoundTrips()
        {
            TestSeedStore.TryParse(TestSeedStore.Format(ulong.MaxValue), out ulong seed);

            Assert.That(seed, Is.EqualTo(ulong.MaxValue));
        }

        #endregion

        #region Persistence

        [Test]
        public void LoadOrCreate_NothingSaved_CreatesAndSavesSeed()
        {
            ulong seed = _seeds.LoadOrCreate();

            Assert.That(seed, Is.LessThan((ulong)TestSeedStore.NewSeedLimit));
            Assert.That(_store.GetString(TestSeedStore.StoreKey, null), Is.EqualTo(TestSeedStore.Format(seed)));
            Assert.That(_store.SaveCount, Is.EqualTo(1));
        }

        [Test]
        public void LoadOrCreate_Twice_ReturnsSameSeed()
        {
            ulong first = _seeds.LoadOrCreate();
            ulong second = new TestSeedStore(_store, new Random(2)).LoadOrCreate();

            Assert.That(second, Is.EqualTo(first));
        }

        [Test]
        public void LoadOrCreate_AfterSave_ReturnsSavedSeed()
        {
            _seeds.Save(ulong.MaxValue);

            Assert.That(_seeds.LoadOrCreate(), Is.EqualTo(ulong.MaxValue));
        }

        [Test]
        public void LoadOrCreate_SavedTextNotASeed_CreatesAndSavesSeed()
        {
            _store.SetString(TestSeedStore.StoreKey, "garbage");

            ulong seed = _seeds.LoadOrCreate();

            Assert.That(_store.GetString(TestSeedStore.StoreKey, null), Is.EqualTo(TestSeedStore.Format(seed)));
        }

        [Test]
        public void Save_WritesToDevice()
        {
            _seeds.Save(5);

            Assert.That(_store.GetString(TestSeedStore.StoreKey, null), Is.EqualTo("5"));
            Assert.That(_store.SaveCount, Is.EqualTo(1));
        }

        [Test]
        public void CreateNew_ReplacesSavedSeed()
        {
            _seeds.Save(ulong.MaxValue);

            ulong seed = _seeds.CreateNew();

            Assert.That(_seeds.LoadOrCreate(), Is.EqualTo(seed));
            Assert.That(seed, Is.Not.EqualTo(ulong.MaxValue));
        }

        #endregion
    }
}
