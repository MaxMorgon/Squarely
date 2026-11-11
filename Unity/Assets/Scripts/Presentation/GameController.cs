using System;
using System.Collections.Generic;
using Squarely.Simulation;
using UnityEngine;

namespace Squarely.Presentation
{
    /// <summary>
    /// Runs one game: owns its sim, passes player commands to it and plays their events on the game's view.
    /// </summary>
    public sealed class GameController : IPiecePlacer
    {
        private readonly GameView _view;
        private readonly GameSetup _setup;

        private Sim _sim;
        private bool _gameEndRaised;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private Sim _simBeforeLastMove;
        private IReadOnlyList<ISimEvent> _lastMoveEvents;
        private Vector2? _lastMoveDropPoint;
#endif

        public bool IsResolved => _sim.IsResolved;

        /// <summary>Whether the player placed at least one piece in this game.</summary>
        public bool HasPlacedPiece { get; private set; }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        /// <summary>Debug only: the result of the last drift check, or null before the first playback ended.</summary>
        public ViewDrift LastDrift { get; private set; }

        /// <summary>Debug only: whether there is a move to replay.</summary>
        public bool CanReplayLastMove => _simBeforeLastMove != null;
#endif

        /// <summary>
        /// The game resolved: raised once, when playback reaches the resolution (of a move, or of the opening deal
        /// if the game resolves on its first deal). Gives the resolution and the final score.
        /// </summary>
        public event Action<GameResolution, int> GameEndEvent;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        /// <summary>Debug only: the debug overlay asked for a fresh game. The controller's owner restarts it.</summary>
        public event Action RestartRequestEvent;
#endif

        /// <param name="view">The game's view. Its owner initializes it, with this controller as its placer, before <see cref="StartGame"/>.</param>
        public GameController(GameView view, GameSetup setup)
        {
            if (view == null)
                throw new ArgumentNullException(nameof(view));

            _view = view;
            _setup = setup ?? throw new ArgumentNullException(nameof(setup));
        }

        #region Commands

        /// <summary>
        /// Starts the game on the initialized view and plays its opening events (the deal) like a move's.
        /// Call once, after subscribing to <see cref="GameEndEvent"/>.
        /// </summary>
        public void StartGame()
        {
            _view.Player.AddHandler(new GameResolvedHandler(OnGameResolved));
            _view.Player.PlaybackEndEvent += OnPlaybackEnded;

            ulong seed = _setup.Seed ?? (ulong)DateTime.Now.Ticks;
            _sim = new Sim(_setup.Config, seed);
            Debug.Log($"New game, seed {seed}.");

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            _view.CreateDebugOverlay().Initialize(this, _view.Player);
#endif

            // The views start empty, as the board and hand are before the deal.
            _view.Player.Play(_sim.OpeningEvents);
        }

        /// <summary>Whether placing the piece at the origin is a legal move.</summary>
        public bool CanPlace(int pieceInstanceId, GridPos origin)
        {
            return _sim.Preview(new PlacePieceCommand(pieceInstanceId, origin)).Success;
        }

        /// <summary>Places the piece and plays the move's events. Whatever is still playing from the previous move snaps to its end first.</summary>
        /// <param name="dropPoint">Where the pointer dropped the piece, in board cell coordinates (<see cref="BoardSpace.CellCoordinates"/>), or null without a pointer.</param>
        public void Place(int pieceInstanceId, GridPos origin, Vector2? dropPoint)
        {
            // Snap before executing, so the playback that ends here is checked against the state it led to.
            _view.Player.Snap();

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Sim before = _sim.Clone();
#endif

            CommandResult result = _sim.Execute(new PlacePieceCommand(pieceInstanceId, origin));

            if (!result.Success)
            {
                Debug.LogWarning($"Placement failed: {result.FailureReason}");
                return;
            }

            HasPlacedPiece = true;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            _simBeforeLastMove = before;
            _lastMoveEvents = result.Events;
            _lastMoveDropPoint = dropPoint;
#endif

            _view.Player.Play(result.Events, dropPoint);
        }

        #endregion

        // A replay of the resolving move reaches the resolution again; the game ends once.
        private void OnGameResolved(GameResolvedEvent resolved)
        {
            if (_gameEndRaised)
                return;

            _gameEndRaised = true;
            GameEndEvent?.Invoke(resolved.Resolution, resolved.FinalScore);
        }

        private void OnPlaybackEnded()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            CheckDrift();
#endif
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        #region Debug

        /// <summary>Debug only: sets the views back to the state before the last move and plays its events again.</summary>
        public void ReplayLastMove()
        {
            if (!CanReplayLastMove)
                return;

            _view.Player.Snap();
            Sim before = _simBeforeLastMove;
            _view.SetState(before.Board, before.Hand, before.Score, before.Combo);
            _view.Player.Play(_lastMoveEvents, _lastMoveDropPoint);
        }

        /// <summary>Debug only: asks the controller's owner for a fresh game, see <see cref="RestartRequestEvent"/>.</summary>
        public void RequestRestart()
        {
            RestartRequestEvent?.Invoke();
        }

        /// <summary>Compares the views with the sim and logs one error listing every difference.</summary>
        private void CheckDrift()
        {
            LastDrift = ViewDrift.Find(_view.GetShownState(), _sim.Board, _sim.Hand, _sim.Score, _sim.Combo);

            if (!LastDrift.IsEmpty)
                Debug.LogError(LastDrift.ToString());
        }

        #endregion
#endif
    }
}
