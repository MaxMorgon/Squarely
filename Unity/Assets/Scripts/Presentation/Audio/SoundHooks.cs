using UnityEngine;

namespace Squarely.Presentation
{
    /// <summary>
    /// The sounds that no sim event drives (input, UI, game end, records). A cue left empty plays no sound.
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
    }
}
