using Squarely.Presentation;
using Squarely.Simulation;
using UnityEngine;

namespace Squarely.App
{
    /// <summary>
    /// Runs one game with the given setup. When it ends it records the high score and shows the game-over screen:
    /// Restart starts a fresh game with the same setup, Menu goes back to the menu. In editor and development builds,
    /// the debug overlay's restart does the same at any time.
    /// </summary>
    public sealed class GameState : IAppState
    {
        private readonly AppContext _context;
        private readonly AppStateMachine _stateMachine;
        private readonly GameSetup _setup;

        private GameView _view;
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
            _view = Object.Instantiate(_context.ScreenPrefabs.Game, _context.ScreenRoot);
            _game = new GameController(_view, _setup);
            _view.Initialize(_setup.Config, _context.Camera, _context.Texts.GameHud, _game);
            _game.GameEndEvent += OnGameEnded;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            _game.RestartRequestEvent += OnRestartRequested;
#endif
            _game.StartGame();
        }

        public void Exit()
        {
            if (_gameOver != null)
            {
                _gameOver.RestartRequestEvent -= OnRestartRequested;
                _gameOver.MenuRequestEvent -= OnMenuRequested;
                Object.Destroy(_gameOver.gameObject);
            }

            _game.GameEndEvent -= OnGameEnded;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            _game.RestartRequestEvent -= OnRestartRequested;
#endif
            Object.Destroy(_view.gameObject);
        }

        #region Game over

        private void OnGameEnded(GameResolution resolution, int finalScore)
        {
            _gameOver = Object.Instantiate(_context.ScreenPrefabs.GameOver, _context.ScreenRoot);
            _gameOver.Initialize(_context.Texts.GameOver);
            _gameOver.RestartRequestEvent += OnRestartRequested;
            _gameOver.MenuRequestEvent += OnMenuRequested;
            _gameOver.SetResult(resolution, finalScore);
            RecordHighScore(finalScore);
        }

        private void RecordHighScore(int finalScore)
        {
            bool isNew = _context.HighScore.Record(finalScore);
            _gameOver.SetHighScore(_context.HighScore.Best.Value, isNew);
        }

        private void OnRestartRequested()
        {
            _stateMachine.ChangeState(new GameState(_context, _stateMachine, _setup));
        }

        private void OnMenuRequested()
        {
            _stateMachine.ChangeState(new MenuState(_context, _stateMachine));
        }

        #endregion
    }
}
