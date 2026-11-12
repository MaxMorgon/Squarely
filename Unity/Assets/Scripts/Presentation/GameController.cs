using System;
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

        public bool IsResolved => _sim.IsResolved;

        /// <summary>Whether the player placed at least one piece in this game.</summary>
        public bool HasPlacedPiece { get; private set; }

        /// <summary>
        /// The game resolved: raised once, when playback reaches the resolution (of a move, or of the opening deal
        /// if the game resolves on its first deal). Gives the resolution and the final score.
        /// </summary>
        public event Action<GameResolution, int> GameEndEvent;

        /// <summary>A move was executed, before its events play. Raised only for moves that succeed.</summary>
        public event Action<ExecutedMove> MoveExecutedEvent;

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
            _view.GameResolveEvent += OnGameResolved;
            _sim = new Sim(_setup.Config, _setup.Seed);

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

            // The copy is only needed by subscribers of MoveExecutedEvent.
            Sim before = MoveExecutedEvent != null ? _sim.Clone() : null;

            CommandResult result = _sim.Execute(new PlacePieceCommand(pieceInstanceId, origin));

            if (!result.Success)
            {
                Debug.LogWarning($"Placement failed: {result.FailureReason}");
                return;
            }

            HasPlacedPiece = true;
            MoveExecutedEvent?.Invoke(new ExecutedMove(before, result.Events, dropPoint));

            _view.Player.Play(result.Events, dropPoint);
        }

        #endregion

        /// <summary>A copy of the game's sim in its current state. Changing the copy does not change the game.</summary>
        public Sim CopySim()
        {
            return _sim.Clone();
        }

        // A replay of the resolving move reaches the resolution again; the game ends once.
        private void OnGameResolved(GameResolvedEvent resolved)
        {
            if (_gameEndRaised)
                return;

            _gameEndRaised = true;
            GameEndEvent?.Invoke(resolved.Resolution, resolved.FinalScore);
        }
    }
}
