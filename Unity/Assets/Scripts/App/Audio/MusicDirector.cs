using Squarely.Presentation;

namespace Squarely.App
{
    /// <summary>
    /// Decides which music plays on which screen. The tracks and fade values come from <see cref="MusicSetup"/>.
    /// With no music player or no setup it does nothing.
    /// </summary>
    public sealed class MusicDirector
    {
        private readonly MusicPlayer _music;
        private readonly MusicSetup _setup;

        public MusicDirector(MusicPlayer music, MusicSetup setup)
        {
            _music = music;
            _setup = setup;
        }

        /// <summary>The menu and the test screen: crossfades to the menu track.</summary>
        public void PlayMenu()
        {
            if (!CanPlay())
                return;

            _music.FadeTo(1f, _setup.CrossfadeSeconds);
            _music.Play(_setup.MenuMusic, _setup.CrossfadeSeconds);
        }

        /// <summary>A game or a replay: crossfades to the game track and brings the music back to full volume.</summary>
        public void PlayGame()
        {
            if (!CanPlay())
                return;

            _music.FadeTo(1f, _setup.CrossfadeSeconds);
            _music.Play(_setup.GameMusic, _setup.CrossfadeSeconds);
        }

        /// <summary>Fades the music down at game over. The track keeps playing.</summary>
        public void FadeForGameOver()
        {
            if (!CanPlay())
                return;

            _music.FadeTo(_setup.GameOverMusicVolume, _setup.GameOverFadeSeconds);
        }

        private bool CanPlay()
        {
            return _music != null && _setup != null;
        }
    }
}
