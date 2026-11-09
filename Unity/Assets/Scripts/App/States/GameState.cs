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
        private readonly AppFlow _flow;
        private readonly AppScreens _screens;
        private readonly Transform _screenRoot;
        private readonly Camera _camera;
        private readonly GameSetup _setup;

        private GameController _game;
        private GameOverScreen _gameOver;

        public GameState(AppFlow flow, AppScreens screens, Transform screenRoot, Camera camera, GameSetup setup)
        {
            _flow = flow;
            _screens = screens;
            _screenRoot = screenRoot;
            _camera = camera;
            _setup = setup;
        }

        public void Enter()
        {
            _game = Object.Instantiate(_screens.Game, _screenRoot);
            _game.GameEnded += OnGameEnded;
            _game.Initialize(_setup, _camera);
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
            _gameOver = Object.Instantiate(_screens.GameOver, _screenRoot);
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
            _flow.GoToMenu();
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
