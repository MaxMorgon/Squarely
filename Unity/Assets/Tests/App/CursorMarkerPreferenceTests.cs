using NUnit.Framework;

namespace Squarely.App.Tests
{
    public sealed class CursorMarkerPreferenceTests
    {
        private MemoryKeyValueStore _store;
        private CursorMarkerPreference _preference;

        [SetUp]
        public void SetUp()
        {
            _store = new MemoryKeyValueStore();
            _preference = new CursorMarkerPreference(_store);
        }

        [Test]
        public void IsOn_NeverSet_IsOff()
        {
            Assert.That(_preference.IsOn, Is.False);
        }

        [Test]
        public void Set_On_IsOnAndSaved()
        {
            _preference.Set(true);

            Assert.That(_preference.IsOn, Is.True);
            Assert.That(_store.SaveCount, Is.EqualTo(1));
        }

        [Test]
        public void IsOn_NewInstanceOnSameStore_ReadsSavedSetting()
        {
            _preference.Set(true);

            var reloaded = new CursorMarkerPreference(_store);

            Assert.That(reloaded.IsOn, Is.True);
        }

        [Test]
        public void Set_ChangedValue_RaisesChangeEventWithNewValue()
        {
            bool? raised = null;
            _preference.ChangeEvent += isOn => raised = isOn;

            _preference.Set(true);

            Assert.That(raised, Is.True);
        }

        [Test]
        public void Set_SameValue_DoesNotRaiseChangeEvent()
        {
            int count = 0;
            _preference.ChangeEvent += _ => count++;

            _preference.Set(false);

            Assert.That(count, Is.EqualTo(0));
        }
    }
}
