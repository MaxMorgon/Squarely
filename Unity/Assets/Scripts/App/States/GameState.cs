using Squarely.Presentation;
using Squarely.Simulation;
using UnityEngine;

namespace Squarely.App
{
    /// <summary>
    /// Runs games with the given setup. When a game ends it shows the game-over screen:
    /// Restart plays again with the same setup, Menu goes back to the menu.
    /// </summary>
    public sealed class GameState : IAppState
    {
        private readonly AppContext _context;
        private readonly AppStateMachine _stateMachine;
        private readonly GameSetup _setup;

        private GameController _game;
        private GameOverScreen _gameOver;

        public GameState(AppContext context, AppStateMachine stateMachine, GameSetup setup)
        {
            _context = context;
            _stateMachine = stateMachine;
            _setup = setup;
        }

        public void Enter()
        {
            _game = Object.Instantiate(_context.ScreenPrefabs.Game, _context.ScreenRoot);
            _game.GameEnded += OnGameEnded;
            _game.Initialize(_setup, _context.Camera);
        }

        public void Exit()
        {
            CloseGameOver();

            _game.GameEnded -= OnGameEnded;
            Object.Destroy(_game.gameObject);
        }

        #region Game over

        private void OnGameEnded(GameResolution resolution, int finalScore)
        {
            _gameOver = Object.Instantiate(_context.ScreenPrefabs.GameOver, _context.ScreenRoot);
            _gameOver.RestartRequested += OnRestartRequested;
            _gameOver.MenuRequested += OnMenuRequested;
            _gameOver.SetResult(resolution, finalScore);
        }

        private void OnRestartRequested()
        {
            CloseGameOver();
            _game.NewGame();
        }

        private void OnMenuRequested()
        {
            _stateMachine.ChangeState(new MenuState(_context, _stateMachine));
        }

        private void CloseGameOver()
        {
            if (_gameOver == null)
                return;

            _gameOver.RestartRequested -= OnRestartRequested;
            _gameOver.MenuRequested -= OnMenuRequested;
            Object.Destroy(_gameOver.gameObject);
            _gameOver = null;
        }

        #endregion
    }
}
