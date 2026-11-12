using UnityEngine;

namespace Squarely.Presentation
{
    /// <summary>
    /// The sounds that no sim event drives (input, UI) and the music setup. A cue left empty plays no sound.
    /// </summary>
    [CreateAssetMenu(menuName = "Squarely/Sound Hooks", fileName = "SoundHooks")]
    public sealed class SoundHooks : ScriptableObject
    {
        [Header("Input")]
        [Tooltip("The player picks a piece up from the hand.")]
        [SerializeField] private SoundCue _piecePickUp;

        [Tooltip("The player drops a piece where it cannot go, and it goes back to the hand.")]
        [SerializeField] private SoundCue _pieceReturn;

        [Tooltip("The ghost moves to a new valid cell during a drag. Set its volume low in the cue.")]
        [SerializeField] private SoundCue _snapPreview;

        [Header("UI")]
        [Tooltip("Any button. One cue for all buttons.")]
        [SerializeField] private SoundCue _buttonTap;

        [SerializeField] private SoundCue _panelOpen;

        [SerializeField] private SoundCue _panelClose;

        [SerializeField] private SoundCue _toggleOn;

        [SerializeField] private SoundCue _toggleOff;

        [Header("Game end")]
        [Tooltip("The game-over screen opens after a lost game.")]
        [SerializeField] private SoundCue _gameLost;

        [Tooltip("The game-over screen opens after a won game.")]
        [SerializeField] private SoundCue _gameWon;

        [Header("Records")]
        [Tooltip("The score goes above the best score during play. Once per game.")]
        [SerializeField] private SoundCue _newBest;

        [Tooltip("The game-over screen shows a new record.")]
        [SerializeField] private SoundCue _newRecord;

        [Header("Music")]
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

        public SoundCue PiecePickUp => _piecePickUp;

        public SoundCue PieceReturn => _pieceReturn;

        public SoundCue SnapPreview => _snapPreview;

        public SoundCue ButtonTap => _buttonTap;

        public SoundCue PanelOpen => _panelOpen;

        public SoundCue PanelClose => _panelClose;

        public SoundCue ToggleOn => _toggleOn;

        public SoundCue ToggleOff => _toggleOff;

        public SoundCue GameLost => _gameLost;

        public SoundCue GameWon => _gameWon;

        public SoundCue NewBest => _newBest;

        public SoundCue NewRecord => _newRecord;

        public AudioClip MenuMusic => _menuMusic;

        public AudioClip GameMusic => _gameMusic;

        public float CrossfadeSeconds => _crossfadeSeconds;

        public float GameOverMusicVolume => _gameOverMusicVolume;

        public float GameOverFadeSeconds => _gameOverFadeSeconds;
    }
}
