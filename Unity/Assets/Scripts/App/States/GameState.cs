using Squarely.Presentation;
using UnityEngine;

namespace Squarely.App
{
    /// <summary>
    /// Runs one game with the given setup.
    /// </summary>
    public sealed class GameState : IAppState
    {
        private readonly GameController _gamePrefab;
        private readonly Transform _screenRoot;
        private readonly Camera _camera;
        private readonly GameSetup _setup;

        private GameController _game;

        public GameState(GameController gamePrefab, Transform screenRoot, Camera camera, GameSetup setup)
        {
            _gamePrefab = gamePrefab;
            _screenRoot = screenRoot;
            _camera = camera;
            _setup = setup;
        }

        public void Enter()
        {
            _game = Object.Instantiate(_gamePrefab, _screenRoot);
            _game.Initialize(_setup, _camera);
        }

        public void Exit()
        {
            Object.Destroy(_game.gameObject);
        }
    }
}
