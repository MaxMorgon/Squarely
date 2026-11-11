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
#endif

        public bool IsResolved => _sim.IsResolved;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        /// <summary>Debug only: the result of the last drift check, or null before the first playback ended.</summary>
        public ViewDrift LastDrift { get; private set; }

        /// <summary>Debug only: whether there is a move to replay.</summary>
        public bool CanReplayLastMove => _simBeforeLastMove != null;
#endif

        /// <summary>
        /// The game resolved: raised once, when playback of the resolving move reaches the resolution,
        /// or right away if the game resolves on its first deal. Gives the resolution and the final score.
        /// </summary>
        public event Action<GameResolution, int> GameEndEvent;

        /// <param name="view">The game's view. Its owner initializes it, with this controller as its placer, before <see cref="StartGame"/>.</param>
        public GameController(GameView view, GameSetup setup)
        {
            if (view == null)
                throw new ArgumentNullException(nameof(view));

            _view = view;
            _setup = setup ?? throw new ArgumentNullException(nameof(setup));
        }

        #region Commands

        /// <summary>Starts the game on the initialized view. Call once, after subscribing to <see cref="GameEndEvent"/>.</summary>
        public void StartGame()
        {
            _view.Player.AddHandler(new GameResolvedHandler(OnGameResolved));
            _view.Player.PlaybackEndEvent += OnPlaybackEnded;

            ulong seed = _setup.Seed ?? (ulong)DateTime.Now.Ticks;
            _sim = new Sim(_setup.Config, seed);
            Debug.Log($"New game, seed {seed}.");

            // The opening deal has no events: the views start from the sim's state.
            SetViewFromSim(_sim);

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            _view.gameObject.AddComponent<PlaybackDebugOverlay>().Initialize(this, _view.Player);
#endif

            if (_sim.IsResolved)
                RaiseGameEnd(_sim.Resolution, _sim.Score);
        }

        /// <summary>Whether placing the piece at the origin is a legal move.</summary>
        public bool CanPlace(int pieceInstanceId, GridPos origin)
        {
            return _sim.Preview(new PlacePieceCommand(pieceInstanceId, origin)).Success;
        }

        /// <summary>Places the piece and plays the move's events. Whatever is still playing from the previous move snaps to its end first.</summary>
        public void Place(int pieceInstanceId, GridPos origin)
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

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            _simBeforeLastMove = before;
            _lastMoveEvents = result.Events;
#endif

            _view.Player.Play(result.Events);
        }

        #endregion

        private void SetViewFromSim(Sim sim)
        {
            _view.SetState(sim.Board, sim.Hand, sim.Score, sim.Combo);
        }

        private void OnGameResolved(GameResolvedEvent resolved)
        {
            RaiseGameEnd(resolved.Resolution, resolved.FinalScore);
        }

        private void RaiseGameEnd(GameResolution resolution, int finalScore)
        {
            if (_gameEndRaised)
                return;

            _gameEndRaised = true;
            GameEndEvent?.Invoke(resolution, finalScore);
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
            SetViewFromSim(_simBeforeLastMove);
            _view.Player.Play(_lastMoveEvents);
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
