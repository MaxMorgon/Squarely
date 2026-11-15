using Squarely.Presentation;

namespace Squarely.App
{
    /// <summary>The app's sound: the sound player, the music and the player's sound settings.</summary>
    public sealed class AppAudio
    {
        /// <summary>Plays sound cues. Lives in the scene, so it survives screen changes.</summary>
        public SoundPlayer Sound { get; }

        /// <summary>Chooses the music of each screen.</summary>
        public MusicDirector Music { get; }

        /// <summary>The sound effects and music toggles of the player.</summary>
        public SoundPreferences Prefs { get; }

        public AppAudio(SoundPlayer sound, MusicDirector music, SoundPreferences prefs)
        {
            Sound = sound;
            Music = music;
            Prefs = prefs;
        }

        /// <summary>Turns sound effects on or off, saves it and plays the toggle sound.</summary>
        public void SetSfx(bool isOn)
        {
            Prefs.SetSfx(isOn);
            PlayToggle(isOn);
        }

        /// <summary>Turns music on or off, saves it and plays the toggle sound.</summary>
        public void SetMusic(bool isOn)
        {
            Prefs.SetMusic(isOn);
            PlayToggle(isOn);
        }

        /// <summary>Plays the toggle-on or toggle-off sound.</summary>
        public void PlayToggle(bool isOn)
        {
            Sound.PlayHook(hooks => isOn ? hooks.ToggleOn : hooks.ToggleOff);
        }
    }
}
