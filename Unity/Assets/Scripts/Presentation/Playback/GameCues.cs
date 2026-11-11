using System;
using UnityEngine;

namespace Squarely.Presentation
{
    /// <summary>The cue sequence for each sim event the game animates.</summary>
    [Serializable]
    public sealed class GameCues
    {
        [SerializeField] private CueSequence _piecePlaced;

        [Tooltip("Plays on every placement, on the board's other filled cells (not the placed piece, not the cells the move clears). " +
                 "Order its cells FromPlacement to spread outward from the piece.")]
        [SerializeField] private CueSequence _boardWave;

        [SerializeField] private CueSequence _patternsCleared;
        [SerializeField] private CueSequence _scoreChanged;
        [SerializeField] private CueSequence _comboChanged;
        [SerializeField] private CueSequence _pieceDealt;

        public CueSequence PiecePlaced => _piecePlaced;

        /// <summary>Played by <see cref="PiecePlacedHandler"/> on every placement, on the filled board cells the move neither placed nor clears.</summary>
        public CueSequence BoardWave => _boardWave;

        public CueSequence PatternsCleared => _patternsCleared;

        public CueSequence ScoreChanged => _scoreChanged;

        public CueSequence ComboChanged => _comboChanged;

        public CueSequence PieceDealt => _pieceDealt;
    }
}
