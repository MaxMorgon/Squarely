using System;
using UnityEngine;

namespace Squarely.Presentation
{
    /// <summary>The cue sequence for each sim event the game animates.</summary>
    [Serializable]
    public sealed class GameCues
    {
        [SerializeField] private CueSequence _piecePlaced;

        [Tooltip("Plays on a placement that clears nothing, on the board's other filled cells (not the placed piece). " +
                 "Order its cells FromPlacement to spread outward from the piece.")]
        [SerializeField] private CueSequence _placementWave;

        [Tooltip("Plays on a placement that clears, on the board's filled cells that stay (the placed piece's too). " +
                 "Order its cells FromPlacement to spread outward from the piece.")]
        [SerializeField] private CueSequence _clearWave;

        [SerializeField] private CueSequence _patternsCleared;
        [SerializeField] private CueSequence _scoreChanged;
        [SerializeField] private CueSequence _comboChanged;
        [SerializeField] private CueSequence _pieceDealt;

        public CueSequence PiecePlaced => _piecePlaced;

        /// <summary>Played by <see cref="PiecePlacedHandler"/> when the move clears nothing, on the filled board cells the move did not place.</summary>
        public CueSequence PlacementWave => _placementWave;

        /// <summary>Played by <see cref="PiecePlacedHandler"/> when the move clears, on the filled board cells the move does not clear.</summary>
        public CueSequence ClearWave => _clearWave;

        public CueSequence PatternsCleared => _patternsCleared;

        public CueSequence ScoreChanged => _scoreChanged;

        public CueSequence ComboChanged => _comboChanged;

        public CueSequence PieceDealt => _pieceDealt;
    }
}
