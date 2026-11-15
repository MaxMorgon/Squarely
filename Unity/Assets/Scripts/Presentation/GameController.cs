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
        private Sim _sim;
        private readonly ReplayRecorder _recorder;

        private bool _gameEndRaised;

        public bool IsResolved => _sim.IsResolved;

        /// <summary>Whether the player placed at least one piece in this game.</summary>
        public bool HasPlacedPiece { get; private set; }

        /// <summary>
        /// The game ended: raised once, after the view reports the resolution (playback, the new record
        /// celebration and the hold time are done). Also for a game that resolves on its first deal. Gives the resolution and the final score.
        /// </summary>
        public event Action<GameResolution, int> GameEndEvent;

#if UNITY_EDITOR || SQUARELY_DEV_TOOLS
        /// <summary>A move was executed, before its events play. Raised only for moves that succeed.</summary>
        public event Action<ExecutedMove> MoveExecutedEvent;
#endif

        /// <summary>Creates the game in its state before the deal is played. Its state can be read at once.</summary>
        /// <param name="view">The game's view. Its owner initializes it, with this controller as its placer, before <see cref="StartGame"/>.</param>
        public GameController(GameView view, GameSetup setup)
        {
            if (view == null)
                throw new ArgumentNullException(nameof(view));

            if (setup == null)
                throw new ArgumentNullException(nameof(setup));

            _view = view;
            _sim = new Sim(setup.Config, setup.Seed);
            _recorder = new ReplayRecorder(_sim);
        }

        #region Commands

        /// <summary>
        /// Starts the game on the initialized view and plays its opening events (the deal) like a move's.
        /// Call once, after subscribing to <see cref="GameEndEvent"/>.
        /// </summary>
        public void StartGame()
        {
            _view.GameResolveEvent += OnGameResolved;

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
            if (!Execute(new PlacePieceCommand(pieceInstanceId, origin), dropPoint))
                return;

            HasPlacedPiece = true;
        }

        /// <summary>
        /// Ends the game at once as a loss, like when no piece fits, and plays the events like a move's.
        /// Does nothing if the game is resolved.
        /// </summary>
        public void EndGame()
        {
            if (IsResolved)
                return;

            Execute(new EndGameCommand(), null);
        }

        /// <summary>
        /// Executes the command like a move and plays its events. Whatever is still playing from the previous move snaps to its end first.
        /// Returns whether it succeeded. A failed command changes nothing.
        /// </summary>
        public bool ExecuteCommand(ISimCommand command)
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));

            return Execute(command, null);
        }

        /// <summary>Executes the command and plays its events. Returns whether it succeeded.</summary>
        private bool Execute(ISimCommand command, Vector2? dropPoint)
        {
            // Snap before executing, so the playback that ends here is checked against the state it led to.
            _view.Player.Snap();

#if UNITY_EDITOR || SQUARELY_DEV_TOOLS
            // The copy is only needed by subscribers of MoveExecutedEvent.
            Sim before = MoveExecutedEvent != null ? _sim.Clone() : null;
#endif

            CommandResult result = _recorder.Execute(command);

            if (!result.Success)
            {
                Debug.LogWarning($"{command} failed: {result.FailureReason}");
                return false;
            }

#if UNITY_EDITOR || SQUARELY_DEV_TOOLS
            MoveExecutedEvent?.Invoke(new ExecutedMove(before, result.Events, dropPoint));
#endif

            _view.Player.Play(result.Events, dropPoint);
            return true;
        }

        #endregion

        /// <summary>The replay of the game so far: every command that succeeded. Fine for a half-finished game. Call after <see cref="StartGame"/>.</summary>
        public GameReplay BuildReplay()
        {
            return _recorder.Build();
        }

#if UNITY_EDITOR || SQUARELY_DEV_TOOLS
        /// <summary>A copy of the game's sim in its current state. Changing the copy does not change the game.</summary>
        public Sim CopySim()
        {
            return _sim.Clone();
        }
#endif

#if UNITY_EDITOR || SQUARELY_DEV_TOOLS
        /// <summary>
        /// Sets the game back to a snapshot at once, with no animation: whatever is playing snaps to its end,
        /// then the views show the snapshot. The game goes on from the snapshot. The snapshot stays unchanged.
        /// </summary>
        /// <param name="snapshot">A copy of the sim from <see cref="CopySim"/>, taken after <paramref name="commandCount"/> successful commands.</param>
        /// <param name="commandCount">How many commands the snapshot has run. The replay of the game keeps only these.</param>
        public void RestoreSim(Sim snapshot, int commandCount)
        {
            if (snapshot == null)
                throw new ArgumentNullException(nameof(snapshot));

            _view.Player.Snap();
            _sim = snapshot.Clone();
            _recorder.Restore(_sim, commandCount);
            _view.SetState(_sim.Board, _sim.Hand, _sim.Score, _sim.Combo);
        }
#endif

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
