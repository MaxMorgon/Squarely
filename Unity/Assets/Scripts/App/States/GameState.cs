using Squarely.Presentation;
using UnityEngine;

namespace Squarely.App
{
    /// <summary>
    /// Runs one game with the given setup.
    /// </summary>
    public sealed class GameState : IAppState
    {
        private readonly AppContext _context;
        private readonly GameSetup _setup;

        private GameController _game;

        public GameState(AppContext context, GameSetup setup)
        {
            _context = context;
            _setup = setup;
        }

        public void Enter()
        {
            _game = Object.Instantiate(_context.ScreenPrefabs.Game, _context.ScreenRoot);
            _game.Initialize(_setup, _context.Camera);
        }

        public void Exit()
        {
            Object.Destroy(_game.gameObject);
        }
    }
}
