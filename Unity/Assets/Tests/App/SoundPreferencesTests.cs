using NUnit.Framework;
using Squarely.Presentation;

namespace Squarely.App.Tests
{
    public sealed class SoundPreferencesTests
    {
        private MemoryKeyValueStore _store;

        [SetUp]
        public void SetUp()
        {
            _store = new MemoryKeyValueStore();
        }

        private SoundPreferences Create()
        {
            // A null mixer is allowed: the settings keep their state and skip the mixer.
            return new SoundPreferences(_store, new SoundSettings(null));
        }

        [Test]
        public void New_NothingSaved_BothOn()
        {
            SoundPreferences preferences = Create();

            Assert.That(preferences.SfxOn, Is.True);
            Assert.That(preferences.MusicOn, Is.True);
        }

        [Test]
        public void SetSfx_Off_KeepsMusicAndSavesAtOnce()
        {
            SoundPreferences preferences = Create();

            preferences.SetSfx(false);

            Assert.That(preferences.SfxOn, Is.False);
            Assert.That(preferences.MusicOn, Is.True);
            Assert.That(_store.GetInt(SoundPreferences.SfxKey, 1), Is.EqualTo(0));
            Assert.That(_store.SaveCount, Is.EqualTo(1));
        }

        [Test]
        public void SetMusic_Off_KeepsSfx()
        {
            SoundPreferences preferences = Create();

            preferences.SetMusic(false);

            Assert.That(preferences.SfxOn, Is.True);
            Assert.That(preferences.MusicOn, Is.False);
        }

        [Test]
        public void New_AfterSaving_LoadsTheSavedToggles()
        {
            SoundPreferences first = Create();
            first.SetSfx(false);
            first.SetMusic(false);

            SoundPreferences second = Create();

            Assert.That(second.SfxOn, Is.False);
            Assert.That(second.MusicOn, Is.False);
        }

        [Test]
        public void SetSfx_BackOn_SavesOn()
        {
            SoundPreferences preferences = Create();
            preferences.SetSfx(false);

            preferences.SetSfx(true);

            Assert.That(Create().SfxOn, Is.True);
        }
    }
}
