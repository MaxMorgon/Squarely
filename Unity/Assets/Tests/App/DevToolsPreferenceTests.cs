using NUnit.Framework;

namespace Squarely.App.Tests
{
    public sealed class DevToolsPreferenceTests
    {
        private MemoryKeyValueStore _store;
        private DevToolsPreference _preference;

        [SetUp]
        public void SetUp()
        {
            _store = new MemoryKeyValueStore();
            _preference = new DevToolsPreference(_store);
        }

        [Test]
        public void IsOn_NeverSet_IsOn()
        {
            Assert.That(_preference.IsOn, Is.True);
        }

        [Test]
        public void Set_Off_IsOffAndSaved()
        {
            _preference.Set(false);

            Assert.That(_preference.IsOn, Is.False);
            Assert.That(_store.SaveCount, Is.EqualTo(1));
        }

        [Test]
        public void IsOn_NewInstanceOnSameStore_ReadsSavedSetting()
        {
            _preference.Set(false);

            var reloaded = new DevToolsPreference(_store);

            Assert.That(reloaded.IsOn, Is.False);
        }

        [Test]
        public void Set_ChangedValue_RaisesChangeEventWithNewValue()
        {
            bool? raised = null;
            _preference.ChangeEvent += isOn => raised = isOn;

            _preference.Set(false);

            Assert.That(raised, Is.False);
        }

        [Test]
        public void Set_SameValue_DoesNotRaiseChangeEvent()
        {
            int count = 0;
            _preference.ChangeEvent += _ => count++;

            _preference.Set(true);

            Assert.That(count, Is.EqualTo(0));
        }

        [Test]
        public void IsActive_TurnedOff_IsFalse()
        {
            _preference.Set(false);

            Assert.That(_preference.IsActive, Is.False);
        }
    }
}
