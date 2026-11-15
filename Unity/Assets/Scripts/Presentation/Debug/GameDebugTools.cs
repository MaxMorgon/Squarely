#if UNITY_EDITOR || SQUARELY_DEV_TOOLS
using System;
using Squarely.Simulation;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Squarely.Presentation
{
    /// <summary>
    /// Editor and dev-tools builds only (`SQUARELY_DEV_TOOLS`): the debug tools of one game. Creates the playback debug overlay, replays
    /// the last move, checks the views against the sim when a playback ends, and relays the overlay's restart.
    /// Not game logic. The game works the same without it.
    /// </summary>
    public sealed class GameDebugTools
    {
        private readonly GameController _game;
        private readonly GameView _view;
        private readonly ReplayCopier _copier;

        private ExecutedMove _lastMove;
        private PlaybackDebugOverlay _overlay;

        /// <summary>The result of the last drift check, or null before the first playback ended.</summary>
        public ViewDrift LastDrift { get; private set; }

        /// <summary>Whether there is a move to replay.</summary>
        public bool CanReplayLastMove => _lastMove != null;

        /// <summary>Whether the game is still running, so it can be ended.</summary>
        public bool CanEndGame => !_game.IsResolved;

        /// <summary>The overlay asked for a fresh game. The owner restarts it.</summary>
        public event Action RestartRequestEvent;

        /// <param name="game">The game to watch.</param>
        /// <param name="view">The game's initialized view.</param>
        /// <param name="copier">Copies replay codes to the clipboard.</param>
        public GameDebugTools(GameController game, GameView view, ReplayCopier copier)
        {
            _game = game ?? throw new ArgumentNullException(nameof(game));
            _view = view != null ? view : throw new ArgumentNullException(nameof(view));
            _copier = copier ?? throw new ArgumentNullException(nameof(copier));
        }

        /// <summary>
        /// Creates the debug overlay (collapsed) and starts watching the game. Can run at any time in a game.
        /// Does nothing if already started. Can start again after <see cref="Stop"/>.
        /// </summary>
        public void Start()
        {
            if (_overlay != null)
                return;

            _game.MoveExecutedEvent += OnMoveExecuted;
            _view.Player.PlaybackEndEvent += OnPlaybackEnded;
            _overlay = _view.CreateDebugOverlay();
            _overlay.Initialize(this, _view.Player);
        }

        /// <summary>Stops watching the game, removes the overlay and sets the playback speed back to normal. Does nothing if not started.</summary>
        public void Stop()
        {
            if (_overlay == null)
                return;

            _game.MoveExecutedEvent -= OnMoveExecuted;
            _view.Player.PlaybackEndEvent -= OnPlaybackEnded;
            _view.Player.Speed = 1f;
            Object.Destroy(_overlay.gameObject);
            _overlay = null;
            _lastMove = null;
            LastDrift = null;
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

        /// <summary>Ends the game at once as a loss. Does nothing once the game is resolved.</summary>
        public void EndGame()
        {
            _game.EndGame();
        }

        /// <summary>Copies the code of the game so far to the clipboard, finished or not.</summary>
        public void CopyReplay()
        {
            _copier.Copy(_game.BuildReplay());
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
