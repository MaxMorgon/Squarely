#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using Squarely.Simulation;
using UnityEngine;

namespace Squarely.Presentation
{
    /// <summary>
    /// Editor and development builds only: the debug tools of one game. Creates the playback debug overlay, replays
    /// the last move, checks the views against the sim when a playback ends, and relays the overlay's restart.
    /// Not game logic. The game works the same without it.
    /// </summary>
    public sealed class GameDebugTools
    {
        private readonly GameController _game;
        private readonly GameView _view;

        private ExecutedMove _lastMove;

        /// <summary>The result of the last drift check, or null before the first playback ended.</summary>
        public ViewDrift LastDrift { get; private set; }

        /// <summary>Whether there is a move to replay.</summary>
        public bool CanReplayLastMove => _lastMove != null;

        /// <summary>The overlay asked for a fresh game. The owner restarts it.</summary>
        public event Action RestartRequestEvent;

        /// <param name="game">The game to watch.</param>
        /// <param name="view">The game's initialized view.</param>
        public GameDebugTools(GameController game, GameView view)
        {
            _game = game ?? throw new ArgumentNullException(nameof(game));
            _view = view != null ? view : throw new ArgumentNullException(nameof(view));
        }

        /// <summary>Creates the debug overlay (collapsed) and starts watching the game. Call once, before the game starts.</summary>
        public void Start()
        {
            _game.MoveExecutedEvent += OnMoveExecuted;
            _view.Player.PlaybackEndEvent += OnPlaybackEnded;
            _view.CreateDebugOverlay().Initialize(this, _view.Player);
        }

        /// <summary>Stops watching the game.</summary>
        public void Stop()
        {
            _game.MoveExecutedEvent -= OnMoveExecuted;
            _view.Player.PlaybackEndEvent -= OnPlaybackEnded;
        }

        #region Commands

        /// <summary>Sets the views back to the state before the last move and plays its events again.</summary>
        public void ReplayLastMove()
        {
            if (!CanReplayLastMove)
                return;

            _view.Player.Snap();
            Sim before = _lastMove.SimBefore;
            _view.SetState(before.Board, before.Hand, before.Score, before.Combo);
            _view.Player.Play(_lastMove.Events, _lastMove.DropPoint);
        }

        /// <summary>Asks the owner for a fresh game, see <see cref="RestartRequestEvent"/>.</summary>
        public void RequestRestart()
        {
            RestartRequestEvent?.Invoke();
        }

        #endregion

        private void OnMoveExecuted(ExecutedMove move)
        {
            _lastMove = move;
        }

        /// <summary>Compares the views with the sim and logs one error listing every difference.</summary>
        private void OnPlaybackEnded()
        {
            Sim sim = _game.CopySim();
            LastDrift = ViewDrift.Find(_view.GetShownState(), sim.Board, sim.Hand, sim.Score, sim.Combo);

            if (!LastDrift.IsEmpty)
                Debug.LogError(LastDrift.ToString());
        }
    }
}
#endif
