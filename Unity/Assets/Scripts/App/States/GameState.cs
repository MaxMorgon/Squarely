using System;
using Squarely.Presentation;
using Squarely.Simulation;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Squarely.App
{
    /// <summary>
    /// Runs one game of the given game mode, against the given high score. Celebrates once when the score goes above
    /// the best score. When the game ends it records the high score and shows the game-over screen:
    /// Restart starts a fresh game with the same mode and seed setting, Menu goes back to the menu.
    /// The HUD's settings panel can resume, restart or quit at any time; restart and quit ask first once a piece
    /// is placed (the panel closes for the question), and abandon the game without recording the high score.
    /// The game is paused while the panel or the question is open.
    /// In editor and development builds, it also runs the game's debug tools; the debug overlay's restart restarts at once.
    /// </summary>
    public sealed class GameState : IAppState
    {
        private readonly AppContext _context;
        private readonly AppStateMachine _stateMachine;
        private readonly SimConfig _config;
        private readonly ulong? _seed;
        private readonly HighScore _highScore;

        private GameView _view;
        private GameController _game;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private GameDebugTools _debugTools;
#endif
        private NewRecordCheck _newRecordCheck;
        private GameOverScreen _gameOver;
        private SettingsPanel _settings;
        private ConfirmDialog _confirm;
        private Action _confirmedAction;

        /// <param name="seed">The seed of the game, or null for a new seed from the context's seed source. Restart does the same again.</param>
        /// <param name="highScore">The best score this game plays against and records to when it ends.</param>
        public GameState(AppContext context, AppStateMachine stateMachine, SimConfig config, ulong? seed, HighScore highScore)
        {
            _context = context;
            _stateMachine = stateMachine;
            _config = config;
            _seed = seed;
            _highScore = highScore;
        }

        public void Enter()
        {
            ulong seed = _seed ?? _context.SeedSource.Next();
            Debug.Log($"New game, seed {seed}.");
            var setup = new GameSetup(_config, seed);

            _view = Object.Instantiate(_context.ScreenPrefabs.Game, _context.ScreenRoot);
            _game = new GameController(_view, setup);
            _view.Initialize(_config, _context.Camera, _context.Texts.GameHud, _game, _game.OnGameResolved);
            _newRecordCheck = new NewRecordCheck(_highScore.Best);

            _view.Hud.SettingsRequestEvent += OnSettingsRequested;
            _view.Hud.Score.ScoreChangeEvent += OnScoreChanged;
            _game.GameEndEvent += OnGameEnded;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            _debugTools = new GameDebugTools(_game, _view);
            _debugTools.RestartRequestEvent += OnRestartRequested;
            _debugTools.Start();
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
            _debugTools.RestartRequestEvent -= OnRestartRequested;
            _debugTools.Stop();
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
            _stateMachine.ChangeState(new GameState(_context, _stateMachine, _config, _seed, _highScore));
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
            UpdatePause();
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

            // The question replaces the panel; No goes back to the game, not to the panel.
            CloseSettingsPanel();

            _confirmedAction = action;
            _confirm = Object.Instantiate(_context.ScreenPrefabs.Confirm, _context.ScreenRoot);
            _confirm.Initialize(_context.Texts.Confirm);
            _confirm.ConfirmRequestEvent += OnConfirmRequested;
            _confirm.CancelRequestEvent += OnCancelRequested;
            UpdatePause();
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
            UpdatePause();
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

        /// <summary>Closes the settings panel and its confirm dialog, if open, and resumes the game.</summary>
        private void CloseSettings()
        {
            CloseConfirm();
            CloseSettingsPanel();
            UpdatePause();
        }

        private void UpdatePause()
        {
            _view.SetPaused(_settings != null || _confirm != null);
        }

        private void CloseSettingsPanel()
        {
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
