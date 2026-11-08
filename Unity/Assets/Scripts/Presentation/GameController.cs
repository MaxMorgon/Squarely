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
        [SerializeField] private Theme _theme;

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

        private SimConfig _config;
        private Sim _sim;

        public bool IsResolved => _sim.IsResolved;

        #region Unity

        private void Start()
        {
            _config = DefaultSimConfig.Create();

            var poolRoot = new GameObject("Cell Visual Pool").transform;
            poolRoot.SetParent(transform, false);

            var catalog = new CellVisualCatalog(_theme.ResolveCellVisual, poolRoot);
            var layout = new BoardLayout(_config.BoardWidth, _config.BoardHeight, _cellSize);

            _background.Build(layout, _theme.EmptyTile);
            _layerView.Initialize(layout, catalog);
            _handView.Initialize(_config.HandSize, catalog, _cellSize);
            _dragController.Initialize(this, _handView, _boardRoot, layout, catalog);

            NewGame();
        }

        #endregion

        #region Commands

        [ContextMenu("New Game")]
        public void NewGame()
        {
            ulong seed = _seed != 0 ? (ulong)_seed : (ulong)DateTime.Now.Ticks;
            _sim = new Sim(_config, seed);
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
