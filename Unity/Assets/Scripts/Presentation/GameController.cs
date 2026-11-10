using System;
using Squarely.Simulation;
using UnityEngine;

namespace Squarely.Presentation
{
    /// <summary>
    /// Owns the sim for one game, sets up the views and passes player commands to the sim.
    /// Does nothing until its owner calls <see cref="Initialize"/>.
    /// </summary>
    public sealed class GameController : MonoBehaviour
    {
        [SerializeField] private CellTheme _cellTheme;

        [Tooltip("Board cell size in world units.")]
        [SerializeField] private float _cellSize = 1f;

        [Tooltip("Parent of the board views. The board is centered on it.")]
        [SerializeField] private Transform _boardRoot;

        [SerializeField] private BoardBackground _background;
        [SerializeField] private LayerView _layerView;
        [SerializeField] private HandView _handView;
        [SerializeField] private PieceDragController _dragController;
        [SerializeField] private ScoreView _scoreView;

        [Tooltip("Edit time only: the game mode whose board and hand size the gizmos preview.")]
        [SerializeField] private SimConfigAsset _gizmoPreviewConfig;

        private GameSetup _setup;
        private BoardSpace _boardSpace;
        private int _handSize;
        private Sim _sim;

        public bool IsResolved => _sim.IsResolved;

        private SimConfig Config => _setup.Config;

        #region Unity

        private void OnDrawGizmos()
        {
            SimConfig config = GizmoConfig();

            if (config == null || config.BoardWidth <= 0 || config.BoardHeight <= 0)
                return;

            if (_background != null)
                _background.DrawGizmos(new BoardSpace(config.BoardWidth, config.BoardHeight, _cellSize));

            if (_handView != null)
                _handView.DrawGizmos(config.HandSize, _cellSize);
        }

        #endregion

        #region Commands

        /// <summary>Sets up the views for the setup's game mode and starts the first game. Call once.</summary>
        /// <param name="camera">The camera that renders the board, used to map the pointer to the board.</param>
        public void Initialize(GameSetup setup, Camera camera)
        {
            _setup = setup ?? throw new ArgumentNullException(nameof(setup));

            var poolRoot = new GameObject("Cell View Pool").transform;
            poolRoot.SetParent(transform, false);

            var viewPool = new CellViewPool(_cellTheme.ResolveCellView, poolRoot);
            _boardSpace = new BoardSpace(Config.BoardWidth, Config.BoardHeight, _cellSize);
            _handSize = Config.HandSize;

            _background.Build(_boardSpace, _cellTheme.EmptyTile);
            _layerView.Initialize(_boardSpace, viewPool);
            _handView.Initialize(_handSize, viewPool, _cellSize);
            _dragController.Initialize(camera, this,_handView, _boardRoot, _boardSpace, viewPool);

            NewGame();
        }

        [ContextMenu("New Game")]
        public void NewGame()
        {
            if (Config.BoardWidth != _boardSpace.Width || Config.BoardHeight != _boardSpace.Height || Config.HandSize != _handSize)
            {
                Debug.LogError("Board or hand size changed since play mode started. Restart play mode to apply it.");
                return;
            }

            ulong seed = _setup.Seed ?? (ulong)DateTime.Now.Ticks;
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
            _handView.SetHand(_sim.Hand);
            _scoreView.SetScore(_sim.Score, _sim.Combo);
        }

        private SimConfig GizmoConfig()
        {
            if (_setup != null)
                return _setup.Config;

            if (_gizmoPreviewConfig != null)
                return _gizmoPreviewConfig.Config;

            return null;
        }
    }
}
