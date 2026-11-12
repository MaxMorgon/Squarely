using Squarely.Presentation;

namespace Squarely.App
{
    /// <summary>
    /// The sound effects and music toggles of the player, saved in a store and applied through <see cref="SoundSettings"/>.
    /// Both are on until the player turns them off.
    /// </summary>
    public sealed class SoundPreferences
    {
        /// <summary>The key the sound effects toggle is saved under. 1 is on, 0 is off.</summary>
        public const string SfxKey = "SfxOn";

        /// <summary>The key the music toggle is saved under. 1 is on, 0 is off.</summary>
        public const string MusicKey = "MusicOn";

        private readonly IKeyValueStore _store;
        private readonly SoundSettings _settings;

        public bool SfxOn => _settings.SfxOn;

        public bool MusicOn => _settings.MusicOn;

        /// <summary>Loads the saved toggles and applies them.</summary>
        public SoundPreferences(IKeyValueStore store, SoundSettings settings)
        {
            _store = store;
            _settings = settings;
            _settings.Set(_store.GetInt(SfxKey, 1) != 0, _store.GetInt(MusicKey, 1) != 0);
        }

        /// <summary>Turns sound effects on or off, applies it and saves it at once.</summary>
        public void SetSfx(bool isOn)
        {
            _settings.Set(isOn, MusicOn);
            Save();
        }

        /// <summary>Turns music on or off, applies it and saves it at once.</summary>
        public void SetMusic(bool isOn)
        {
            _settings.Set(SfxOn, isOn);
            Save();
        }

        private void Save()
        {
            _store.SetInt(SfxKey, SfxOn ? 1 : 0);
            _store.SetInt(MusicKey, MusicOn ? 1 : 0);
            _store.Save();
        }
    }
}
