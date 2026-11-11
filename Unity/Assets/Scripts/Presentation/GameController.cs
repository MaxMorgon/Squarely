using System;
using Squarely.Simulation;
using UnityEngine;

namespace Squarely.Presentation
{
    /// <summary>
    /// Runs one game: owns its sim, passes player commands to it and keeps the game's view up to date.
    /// </summary>
    public sealed class GameController
    {
        private readonly GameView _view;
        private readonly GameSetup _setup;
        private readonly Camera _camera;
        private readonly GameHudTexts _hudTexts;

        private Sim _sim;

        public bool IsResolved => _sim.IsResolved;

        /// <summary>The game resolved, after a move or right on the first deal. Gives the resolution and the final score.</summary>
        public event Action<GameResolution, int> GameEndEvent;

        /// <param name="view">The game's scene objects, not yet initialized.</param>
        /// <param name="camera">The camera that renders the board, used to map the pointer to the board.</param>
        /// <param name="hudTexts">The texts the game's HUD shows.</param>
        public GameController(GameView view, GameSetup setup, Camera camera, GameHudTexts hudTexts)
        {
            if (view == null)
                throw new ArgumentNullException(nameof(view));

            _view = view;
            _setup = setup ?? throw new ArgumentNullException(nameof(setup));
            _camera = camera;
            _hudTexts = hudTexts;
        }

        #region Commands

        /// <summary>Sets up the view for the setup's game mode and starts the game. Call once, after subscribing to <see cref="GameEndEvent"/>.</summary>
        public void StartGame()
        {
            _view.Initialize(_setup.Config, _camera, this, _hudTexts);

            ulong seed = _setup.Seed ?? (ulong)DateTime.Now.Ticks;
            _sim = new Sim(_setup.Config, seed);
            Debug.Log($"New game, seed {seed}.");

            RefreshView();

            if (_sim.IsResolved)
                GameEndEvent?.Invoke(_sim.Resolution, _sim.Score);
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

            // Until event playback exists, the view jumps straight to the sim's final state.
            RefreshView();

            if (_sim.IsResolved)
                GameEndEvent?.Invoke(_sim.Resolution, _sim.Score);
        }

        #endregion

        private void RefreshView()
        {
            _view.SetState(_sim.Board, _sim.Hand, _sim.Score, _sim.Combo);
        }
    }
}
