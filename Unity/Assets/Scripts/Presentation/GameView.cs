using System;
using Squarely.Simulation;
using UnityEngine;

namespace Squarely.Presentation
{
    /// <summary>
    /// The game's scene objects: board, hand, input, HUD and their settings. Builds the views for a game mode,
    /// sets them from a sim state, and plays commands' events on them through its <see cref="Player"/>.
    /// Does nothing until its owner calls <see cref="Initialize"/>.
    /// </summary>
    public sealed class GameView : MonoBehaviour
    {
        [SerializeField] private CellTheme _cellTheme;

        [Tooltip("How each sim event is animated.")]
        [SerializeField] private GameCues _cues;

        [Tooltip("Board cell size in world units.")]
        [SerializeField] private float _cellSize = 1f;

        [Tooltip("Parent of the board views. The board is centered on it.")]
        [SerializeField] private Transform _boardRoot;

        [SerializeField] private BoardBackground _background;
        [SerializeField] private LayerView _layerView;
        [SerializeField] private HandView _handView;
        [SerializeField] private PieceDragController _dragController;
        [SerializeField] private GameHud _hud;

        [Tooltip("Editor and development builds only: the playback debug panel created for each game.")]
        [SerializeField] private PlaybackDebugOverlay _debugOverlayPrefab;

        [Tooltip("Edit time only: the game mode whose board and hand size the gizmos preview.")]
        [SerializeField] private SimConfigAsset _gizmoPreviewConfig;

        private SimConfig _config;
        private BoardSpace _boardSpace;
        private int _handSize;

        /// <summary>Plays sim events on the views. Exists after <see cref="Initialize"/>.</summary>
        public EventPlayer Player { get; private set; }

        /// <summary>The in-game HUD: the score, the settings button and the "new record" celebration.</summary>
        public GameHud Hud => _hud;

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

        /// <summary>
        /// Builds the board, hand and HUD views for the game mode, connects drag input to the placer
        /// and creates the event player with a handler per event. Call once.
        /// </summary>
        /// <param name="camera">The camera that renders the board, used to map the pointer to the board.</param>
        /// <param name="hudTexts">The texts the HUD shows.</param>
        /// <param name="placer">Receives the player's placements.</param>
        /// <param name="onGameResolved">Called when playback reaches the game's resolution.</param>
        public void Initialize(SimConfig config, Camera camera, GameHudTexts hudTexts, IPiecePlacer placer, Action<GameResolvedEvent> onGameResolved)
        {
            _config = config;

            var poolRoot = new GameObject("Cell View Pool").transform;
            poolRoot.SetParent(transform, false);

            var viewPool = new CellViewPool(_cellTheme.ResolveCellView, poolRoot);
            var boardSpace = new BoardSpace(config.BoardWidth, config.BoardHeight, _cellSize);
            _boardSpace = boardSpace;
            _handSize = config.HandSize;

            _background.Build(boardSpace, _cellTheme.EmptyTile);
            _layerView.Initialize(boardSpace, viewPool);
            _handView.Initialize(config.HandSize, viewPool, _cellSize);
            _dragController.Initialize(camera, placer, _handView, _boardRoot, boardSpace, viewPool);
            _hud.Initialize(hudTexts);

            var playbackViews = new PlaybackViews(_layerView, _handView, _hud.Score, transform);
            Player = new EventPlayer(playbackViews, destroyCancellationToken);
            Player.AddHandler(new PiecePlacedHandler(_cues.PiecePlaced, _cues.PlacementWave, _cues.ClearWave));
            Player.AddHandler(new PatternsClearedHandler(_cues.PatternsCleared));
            Player.AddHandler(new ScoreChangedHandler(_cues.ScoreChanged));
            Player.AddHandler(new ComboChangedHandler(_cues.ComboChanged));
            Player.AddHandler(new PieceDealtHandler(_cues.PieceDealt));
            Player.AddHandler(new GameResolvedHandler(onGameResolved));
        }

        /// <summary>
        /// Pauses or resumes the game: while paused, event playback holds and the hand cannot be dragged.
        /// The score count-up and combo are playback steps, so they hold too. Screens above the game keep running.
        /// </summary>
        public void SetPaused(bool isPaused)
        {
            Player.IsPaused = isPaused;
            _dragController.IsBlocked = isPaused;
        }

        /// <summary>Sets the board, the hand, the score and the combo straight from a sim state, without playback.</summary>
        public void SetState(IReadOnlyBoard board, IReadOnlyHand hand, int score, int combo)
        {
            _layerView.Rebuild(board);
            _handView.SetHand(hand);
            _hud.Score.SetScore(score);
            _hud.Score.SetCombo(combo);
        }

        /// <summary>What the board, hand and score views show right now.</summary>
        public ShownState GetShownState()
        {
            var cells = new Grid<string>(_boardSpace.Width, _boardSpace.Height);

            for (int y = 0; y < cells.Height; y++)
            {
                for (int x = 0; x < cells.Width; x++)
                {
                    var pos = new GridPos(x, y);
                    CellView view = _layerView.GetView(pos);
                    cells[pos] = view != null ? view.TypeId : null;
                }
            }

            var hand = new PieceInstance[_handSize];

            for (int slot = 0; slot < hand.Length; slot++)
                hand[slot] = _handView.GetPiece(slot);

            return new ShownState(cells, hand, _hud.Score.Score, _hud.Score.Combo);
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        /// <summary>Debug only: creates the playback debug overlay as a child of this view. Its creator initializes it.</summary>
        public PlaybackDebugOverlay CreateDebugOverlay()
        {
            return Instantiate(_debugOverlayPrefab, transform);
        }
#endif

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
