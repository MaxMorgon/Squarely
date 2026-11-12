using UnityEngine;

namespace Squarely.App
{
    /// <summary>The music tracks and fade values. The only place that holds music setup. <see cref="MusicDirector"/> reads it.</summary>
    [CreateAssetMenu(menuName = "Squarely/Music Setup", fileName = "MusicSetup")]
    public sealed class MusicSetup : ScriptableObject
    {
        [Tooltip("Plays in the menu and on the test screen.")]
        [SerializeField] private AudioClip _menuMusic;

        [Tooltip("Plays in a game and in a replay.")]
        [SerializeField] private AudioClip _gameMusic;

        [Tooltip("Seconds to crossfade between tracks, and to bring the music back up for a new game.")]
        [SerializeField] private float _crossfadeSeconds = 1f;

        [Tooltip("Music volume at game over, from 0 to 1. The track keeps playing.")]
        [Range(0f, 1f)]
        [SerializeField] private float _gameOverMusicVolume = 0.3f;

        [Tooltip("Seconds to fade the music down at game over.")]
        [SerializeField] private float _gameOverFadeSeconds = 1.5f;

        public AudioClip MenuMusic => _menuMusic;

        public AudioClip GameMusic => _gameMusic;

        public float CrossfadeSeconds => _crossfadeSeconds;

        public float GameOverMusicVolume => _gameOverMusicVolume;

        public float GameOverFadeSeconds => _gameOverFadeSeconds;
    }
}
