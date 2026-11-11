using System;
using Squarely.Presentation;
using Squarely.Simulation;
using Object = UnityEngine.Object;

namespace Squarely.App
{
    /// <summary>
    /// Runs one game with the given setup, against the given high score. Celebrates once when the score goes above
    /// the best score. When the game ends it records the high score and shows the game-over screen:
    /// Restart starts a fresh game with the same setup, Menu goes back to the menu.
    /// The HUD's settings panel can resume, restart or quit at any time; restart and quit ask first once a piece
    /// is placed, and abandon the game without recording the high score.
    /// In editor and development builds, the debug overlay's restart restarts at once.
    /// </summary>
    public sealed class GameState : IAppState
    {
        private readonly AppContext _context;
        private readonly AppStateMachine _stateMachine;
        private readonly GameSetup _setup;
        private readonly HighScore _highScore;

        private GameView _view;
        private GameController _game;
        private NewRecordCheck _newRecordCheck;
        private GameOverScreen _gameOver;
        private SettingsPanel _settings;
        private ConfirmDialog _confirm;
        private Action _confirmedAction;

        /// <param name="highScore">The best score this game plays against and records to when it ends.</param>
        public GameState(AppContext context, AppStateMachine stateMachine, GameSetup setup, HighScore highScore)
        {
            _context = context;
            _stateMachine = stateMachine;
            _setup = setup;
            _highScore = highScore;
        }

        public void Enter()
        {
            _view = Object.Instantiate(_context.ScreenPrefabs.Game, _context.ScreenRoot);
            _game = new GameController(_view, _setup);
            _view.Initialize(_setup.Config, _context.Camera, _context.Texts.GameHud, _game);
            _newRecordCheck = new NewRecordCheck(_highScore.Best);

            _view.Hud.SettingsRequestEvent += OnSettingsRequested;
            _view.Hud.Score.ScoreChangeEvent += OnScoreChanged;
            _game.GameEndEvent += OnGameEnded;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            _game.RestartRequestEvent += OnRestartRequested;
#endif
            _game.StartGame();
        }

        public void Exit()
        {
            CloseSettings();

            if (_gameOver != null)
            {
                _gameOver.RestartRequestEvent -= OnRestartRequested;
                _gameOver.MenuRequestEvent -= OnMenuRequested;
                Object.Destroy(_gameOver.gameObject);
            }

            _view.Hud.SettingsRequestEvent -= OnSettingsRequested;
            _view.Hud.Score.ScoreChangeEvent -= OnScoreChanged;
            _game.GameEndEvent -= OnGameEnded;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            _game.RestartRequestEvent -= OnRestartRequested;
#endif
            Object.Destroy(_view.gameObject);
        }

        private void OnScoreChanged(int score)
        {
            // The count-up can run on under the game-over screen, which has its own high score message.
            if (_gameOver != null)
                return;

            if (_newRecordCheck.Check(score))
                _view.Hud.CelebrateNewRecord();
        }

        private void OnRestartRequested()
        {
            _stateMachine.ChangeState(new GameState(_context, _stateMachine, _setup, _highScore));
        }

        private void OnMenuRequested()
        {
            _stateMachine.ChangeState(new MenuState(_context, _stateMachine));
        }

        #region Game over

        private void OnGameEnded(GameResolution resolution, int finalScore)
        {
            // The game is over: the settings panel has nothing left to resume, restart or quit.
            CloseSettings();

            _gameOver = Object.Instantiate(_context.ScreenPrefabs.GameOver, _context.ScreenRoot);
            _gameOver.Initialize(_context.Texts.GameOver);
            _gameOver.RestartRequestEvent += OnRestartRequested;
            _gameOver.MenuRequestEvent += OnMenuRequested;
            _gameOver.SetResult(resolution, finalScore);
            RecordHighScore(finalScore);
        }

        private void RecordHighScore(int finalScore)
        {
            bool isNew = _highScore.Record(finalScore);
            _gameOver.SetHighScore(_highScore.Best.Value, isNew);
        }

        #endregion

        #region Settings

        private void OnSettingsRequested()
        {
            if (_settings != null || _gameOver != null)
                return;

            _settings = Object.Instantiate(_context.ScreenPrefabs.Settings, _context.ScreenRoot);
            _settings.Initialize(_context.Texts.Settings, true);
            _settings.CloseRequestEvent += OnSettingsCloseRequested;
            _settings.RestartRequestEvent += OnSettingsRestartRequested;
            _settings.QuitRequestEvent += OnSettingsQuitRequested;
        }

        private void OnSettingsCloseRequested()
        {
            CloseSettings();
        }

        // Restart and Quit abandon the game: only a game that ends records the high score.
        private void OnSettingsRestartRequested()
        {
            ConfirmThen(OnRestartRequested);
        }

        private void OnSettingsQuitRequested()
        {
            ConfirmThen(OnMenuRequested);
        }

        /// <summary>Does the action at once if no piece is placed yet; otherwise asks "Are you sure?" first.</summary>
        private void ConfirmThen(Action action)
        {
            if (!_game.HasPlacedPiece)
            {
                action();
                return;
            }

            if (_confirm != null)
                return;

            _confirmedAction = action;
            _confirm = Object.Instantiate(_context.ScreenPrefabs.Confirm, _context.ScreenRoot);
            _confirm.Initialize(_context.Texts.Confirm);
            _confirm.ConfirmRequestEvent += OnConfirmRequested;
            _confirm.CancelRequestEvent += OnCancelRequested;
        }

        private void OnConfirmRequested()
        {
            Action action = _confirmedAction;
            CloseConfirm();
            action();
        }

        private void OnCancelRequested()
        {
            CloseConfirm();
        }

        private void CloseConfirm()
        {
            if (_confirm == null)
                return;

            _confirm.ConfirmRequestEvent -= OnConfirmRequested;
            _confirm.CancelRequestEvent -= OnCancelRequested;
            Object.Destroy(_confirm.gameObject);
            _confirm = null;
            _confirmedAction = null;
        }

        /// <summary>Closes the settings panel and its confirm dialog, if open.</summary>
        private void CloseSettings()
        {
            CloseConfirm();

            if (_settings == null)
                return;

            _settings.CloseRequestEvent -= OnSettingsCloseRequested;
            _settings.RestartRequestEvent -= OnSettingsRestartRequested;
            _settings.QuitRequestEvent -= OnSettingsQuitRequested;
            Object.Destroy(_settings.gameObject);
            _settings = null;
        }

        #endregion
    }
}
