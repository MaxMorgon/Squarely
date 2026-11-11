using Squarely.Simulation;
using UnityEngine;

namespace Squarely.Presentation
{
    /// <summary>
    /// The game's scene objects: board, hand, input and their settings. Builds the views for a game mode
    /// and shows the board and hand it is given. Does nothing until its owner calls <see cref="Initialize"/>.
    /// </summary>
    public sealed class GameView : MonoBehaviour
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

        private SimConfig _config;

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

        /// <summary>Builds the board, hand and HUD views for the game mode and connects drag input to the game. Call once.</summary>
        /// <param name="camera">The camera that renders the board, used to map the pointer to the board.</param>
        /// <param name="hudTexts">The texts the HUD shows.</param>
        public void Initialize(SimConfig config, Camera camera, GameController game, GameHudTexts hudTexts)
        {
            _config = config;

            var poolRoot = new GameObject("Cell View Pool").transform;
            poolRoot.SetParent(transform, false);

            var viewPool = new CellViewPool(_cellTheme.ResolveCellView, poolRoot);
            var boardSpace = new BoardSpace(config.BoardWidth, config.BoardHeight, _cellSize);

            _background.Build(boardSpace, _cellTheme.EmptyTile);
            _layerView.Initialize(boardSpace, viewPool);
            _handView.Initialize(config.HandSize, viewPool, _cellSize);
            _dragController.Initialize(camera, game, _handView, _boardRoot, boardSpace, viewPool);
            _scoreView.Initialize(hudTexts);
        }

        /// <summary>Shows the board, the hand, the score and the combo.</summary>
        public void SetState(IReadOnlyBoard board, IReadOnlyHand hand, int score, int combo)
        {
            _layerView.Rebuild(board);
            _handView.SetHand(hand);
            _scoreView.SetScore(score, combo);
        }

        private SimConfig GizmoConfig()
        {
            if (_config != null)
                return _config;

            if (_gizmoPreviewConfig != null)
                return _gizmoPreviewConfig.Config;

            return null;
        }
    }
}
