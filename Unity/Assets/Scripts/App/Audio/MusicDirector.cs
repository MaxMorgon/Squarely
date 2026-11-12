using Squarely.Presentation;

namespace Squarely.App
{
    /// <summary>
    /// Decides which music plays on which screen. The tracks and fade values come from <see cref="SoundHooks"/>.
    /// With no music player or no hooks it does nothing.
    /// </summary>
    public sealed class MusicDirector
    {
        private readonly MusicPlayer _music;
        private readonly SoundHooks _hooks;

        public MusicDirector(MusicPlayer music, SoundHooks hooks)
        {
            _music = music;
            _hooks = hooks;
        }

        /// <summary>The menu and the test screen: crossfades to the menu track.</summary>
        public void PlayMenu()
        {
            if (!CanPlay())
                return;

            _music.FadeTo(1f, _hooks.CrossfadeSeconds);
            _music.Play(_hooks.MenuMusic, _hooks.CrossfadeSeconds);
        }

        /// <summary>A game or a replay: crossfades to the game track and brings the music back to full volume.</summary>
        public void PlayGame()
        {
            if (!CanPlay())
                return;

            _music.FadeTo(1f, _hooks.CrossfadeSeconds);
            _music.Play(_hooks.GameMusic, _hooks.CrossfadeSeconds);
        }

        /// <summary>Fades the music down at game over. The track keeps playing.</summary>
        public void FadeForGameOver()
        {
            if (!CanPlay())
                return;

            _music.FadeTo(_hooks.GameOverMusicVolume, _hooks.GameOverFadeSeconds);
        }

        private bool CanPlay()
        {
            return _music != null && _hooks != null;
        }
    }
}
