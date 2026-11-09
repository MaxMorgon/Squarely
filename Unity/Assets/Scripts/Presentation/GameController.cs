using System;
using Squarely.Simulation;
using UnityEngine;

namespace Squarely.Presentation
{
    /// <summary>
    /// Owns the sim for one game, sets up the views and passes player commands to the sim.
    /// </summary>
    public sealed class GameController : MonoBehaviour
    {
        [Tooltip("The game mode. Edits apply live, also in play mode. Board and hand size changes need a play mode restart.")]
        [SerializeField] private SimConfigAsset _simConfig;

        [SerializeField] private CellTheme _cellTheme;

        [Tooltip("0 picks a new seed for every game.")]
        [SerializeField] private int _seed;

        [Tooltip("Board cell size in world units.")]
        [SerializeField] private float _cellSize = 1f;

        [Tooltip("Parent of the board views. The board is centered on it.")]
        [SerializeField] private Transform _boardRoot;

        [SerializeField] private BoardBackground _background;
        [SerializeField] private LayerView _layerView;
        [SerializeField] private HandView _handView;
        [SerializeField] private PieceDragController _dragController;

        private BoardSpace _boardSpace;
        private int _handSize;
        private Sim _sim;

        public bool IsResolved => _sim.IsResolved;

        private SimConfig Config => _simConfig.Config;

        #region Unity

        private void Start()
        {
            var poolRoot = new GameObject("Cell View Pool").transform;
            poolRoot.SetParent(transform, false);

            var viewPool = new CellViewPool(_cellTheme.ResolveCellView, poolRoot);
            _boardSpace = new BoardSpace(Config.BoardWidth, Config.BoardHeight, _cellSize);
            _handSize = Config.HandSize;

            _background.Build(_boardSpace, _cellTheme.EmptyTile);
            _layerView.Initialize(_boardSpace, viewPool);
            _handView.Initialize(_handSize, viewPool, _cellSize);
            _dragController.Initialize(this, _handView, _boardRoot, _boardSpace, viewPool);

            NewGame();
        }

        private void OnDrawGizmos()
        {
            if (_simConfig == null || Config.BoardWidth <= 0 || Config.BoardHeight <= 0)
                return;

            if (_background != null)
                _background.DrawGizmos(new BoardSpace(Config.BoardWidth, Config.BoardHeight, _cellSize));

            if (_handView != null)
                _handView.DrawGizmos(Config.HandSize, _cellSize);
        }

        #endregion

        #region Commands

        [ContextMenu("New Game")]
        public void NewGame()
        {
            if (Config.BoardWidth != _boardSpace.Width || Config.BoardHeight != _boardSpace.Height || Config.HandSize != _handSize)
            {
                Debug.LogError("Board or hand size changed since play mode started. Restart play mode to apply it.");
                return;
            }

            ulong seed = _seed != 0 ? (ulong)_seed : (ulong)DateTime.Now.Ticks;
            _sim = new Sim(Config, seed);
            Debug.Log($"New game, seed {seed}.");

            RefreshViews();
        }

        /// <summary>Whether placing the piece at the origin is a legal move.</summary>
        public bool CanPlace(int pieceInstanceId, GridPos origin)
        {
            return _sim.Preview(new PlacePieceCommand(pieceInstanceId, origin)).Success;
        }

        public void Place(int pieceInstanceId, GridPos origin)
        {
            CommandResult result = _sim.Execute(new PlacePieceCommand(pieceInstanceId, origin));

            if (!result.Success)
            {
                Debug.LogWarning($"Placement failed: {result.FailureReason}");
                return;
            }

            // Until event playback exists, the views jump straight to the sim's final state.
            RefreshViews();

            if (_sim.IsResolved)
                Debug.Log($"Game over: {_sim.Resolution}, score {_sim.Score}. Use the New Game context menu to restart.");
        }

        #endregion

        private void RefreshViews()
        {
            _layerView.Rebuild(_sim.Board);
            _handView.Show(_sim.Hand);
        }
    }
}
