using System;
using UnityEngine;

namespace Squarely.Presentation
{
    /// <summary>The cue sequence for each sim event the game animates.</summary>
    [Serializable]
    public sealed class GameCues
    {
        [SerializeField] private CueSequence _piecePlaced;
        [SerializeField] private CueSequence _patternsCleared;
        [SerializeField] private CueSequence _scoreChanged;
        [SerializeField] private CueSequence _comboChanged;
        [SerializeField] private CueSequence _pieceDealt;

        public CueSequence PiecePlaced => _piecePlaced;

        public CueSequence PatternsCleared => _patternsCleared;

        public CueSequence ScoreChanged => _scoreChanged;

        public CueSequence ComboChanged => _comboChanged;

        public CueSequence PieceDealt => _pieceDealt;
    }
}
