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
    /// Restart starts a fresh game with the same mode, seed setting and back target; Menu goes back to the back target.
    /// The back target is the state the game was started from (the menu or the test screen). Quit and the Menu button go there.
    /// The HUD's settings panel can resume, restart or quit at any time; restart and quit ask first once a piece
    /// is placed (the panel closes for the question), and abandon the game without recording the high score.
    /// The game is paused while the panel or the question is open. A test game's panel has no "Dev tools" toggle.
    /// Every game is saved in the replay history when it ends, or when the player leaves it after placing a piece.
    /// In the editor and in builds with dev tools, it also runs the game's debug tools while the "Dev tools" setting is on (it follows the setting at once); the debug overlay's restart restarts at once.
    /// </summary>
    public sealed class GameState : IAppState
    {
        private readonly AppContext _context;
        private readonly AppStateMachine _stateMachine;
        private readonly SimConfig _config;
        private readonly ulong? _seed;
        private readonly HighScore _highScore;
        private readonly Func<IAppState> _createBackState;
        private readonly bool _isTest;

        private GameView _view;
        private GameController _game;
#if UNITY_EDITOR || SQUARELY_DEV_TOOLS
        private GameDebugTools _debugTools;
#endif
        private NewRecordCheck _newRecordCheck;
        private GameOverScreen _gameOver;
        private SettingsPanel _settings;
        private readonly ConfirmPrompt _confirm;
        private bool _replayRecorded;

        /// <param name="seed">The seed of the game, or null for a new seed from the context's seed source. Restart does the same again.</param>
        /// <param name="highScore">The best score this game plays against and records to when it ends.</param>
        /// <param name="createBackState">Creates the state to go back to when the player quits or leaves the game over screen. Restart passes it on.</param>
        /// <param name="isTest">True for a game started from the test screen. Its settings panel has no dev tools toggle. Restart passes it on.</param>
        public GameState(AppContext context, AppStateMachine stateMachine, SimConfig config, ulong? seed, HighScore highScore, Func<IAppState> createBackState, bool isTest = false)
        {
            _context = context;
            _stateMachine = stateMachine;
            _config = config;
            _seed = seed;
            _highScore = highScore;
            _createBackState = createBackState ?? throw new ArgumentNullException(nameof(createBackState));
            _isTest = isTest;
            _confirm = new ConfirmPrompt(context.Screens, context.Texts.Confirm);
        }

        public void Enter()
        {
            ulong seed = _seed ?? _context.Services.SeedSource.Next();
            Debug.Log($"New game, seed {seed}.");
            var setup = new GameSetup(_config, seed);

            (_view, _game) = _context.Screens.CreateGame(setup);
            _newRecordCheck = new NewRecordCheck(_highScore.Best);

            _view.Hud.SettingsRequestEvent += OnSettingsRequested;
            _view.Hud.Score.ScoreChangeEvent += OnScoreChanged;
            _game.GameEndEvent += OnGameEnded;
#if UNITY_EDITOR || SQUARELY_DEV_TOOLS
            _debugTools = new GameDebugTools(_game, _view, _context.Services.ReplayCopier);
            _debugTools.RestartRequestEvent += OnRestartRequested;
            _context.Services.DevTools.ChangeEvent += OnDevToolsChanged;

            if (_context.Services.DevTools.IsOn)
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
#if UNITY_EDITOR || SQUARELY_DEV_TOOLS
            _debugTools.RestartRequestEvent -= OnRestartRequested;
            _context.Services.DevTools.ChangeEvent -= OnDevToolsChanged;
            _debugTools.Stop();
#endif
            // A game left before it ended is saved here, if the player placed a piece.
            RecordReplay();

            Object.Destroy(_view.gameObject);
        }

        private void OnScoreChanged(int score)
        {
            // No celebration under the game-over screen (a debug replay can still run a count-up).
            if (_gameOver != null)
                return;

            if (_newRecordCheck.Check(score))
            {
                _context.Audio.Sound.PlayHook(hooks => hooks.NewBest);
                _view.Hud.CelebrateNewRecord();
            }
        }

#if UNITY_EDITOR || SQUARELY_DEV_TOOLS
        private void OnDevToolsChanged(bool isOn)
        {
            if (isOn)
                _debugTools.Start();
            else
                _debugTools.Stop();
        }
#endif

        private void OnRestartRequested()
        {
            _stateMachine.ChangeState(new GameState(_context, _stateMachine, _config, _seed, _highScore, _createBackState, _isTest));
        }

        private void OnMenuRequested()
        {
            _stateMachine.ChangeState(_createBackState());
        }

        #region Game over

        private void OnGameEnded(GameResolution resolution, int finalScore)
        {
            // The game is over: the settings panel has nothing left to resume, restart or quit.
            CloseSettings();

            _context.Audio.Music.FadeForGameOver();
            _gameOver = _context.Screens.CreateScreen(_context.Screens.Prefabs.GameOver);
            _gameOver.Initialize(_context.Texts.GameOver);
            _gameOver.RestartRequestEvent += OnRestartRequested;
            _gameOver.MenuRequestEvent += OnMenuRequested;
            _gameOver.SetResult(resolution, finalScore);
            bool isNewRecord = RecordHighScore(finalScore);
            PlayGameOverJingle(resolution.Outcome, isNewRecord);
            RecordReplay();
#if UNITY_EDITOR || SQUARELY_DEV_TOOLS
            if (_context.Services.DevTools.IsOn)
                _gameOver.SetReplay(_context.Services.ReplayCopier, _game.BuildReplay());
#endif
        }

        /// <summary>Saves the game in the replay history, once.</summary>
        private void RecordReplay()
        {
            if (_replayRecorded)
                return;

            _replayRecorded = true;
            _context.Services.ReplayHistory.Record(_game.BuildReplay());
        }

        private bool RecordHighScore(int finalScore)
        {
            bool isNew = _highScore.Record(finalScore);
            _gameOver.SetHighScore(_highScore.Best.Value, isNew);
            return isNew;
        }

        private void PlayGameOverJingle(GameOutcome outcome, bool isNewRecord)
        {
            switch (GameOverJingle.Choose(outcome, isNewRecord))
            {
                case GameOverJingle.Kind.NewRecord:
                    _context.Audio.Sound.PlayHook(hooks => hooks.NewRecord);
                    break;
                case GameOverJingle.Kind.Won:
                    _context.Audio.Sound.PlayHook(hooks => hooks.GameWon);
                    break;
                default:
                    _context.Audio.Sound.PlayHook(hooks => hooks.GameLost);
                    break;
            }
        }

        #endregion

        #region Settings

        private void OnSettingsRequested()
        {
            if (_settings != null || _gameOver != null)
                return;

            _settings = _context.Screens.CreateSettings(isInGame: true, showDevTools: !_isTest);
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

            if (_confirm.IsOpen)
                return;

            // The question replaces the panel; No goes back to the game, not to the panel.
            CloseSettingsPanel();

            _confirm.Ask(null, action, UpdatePause);
            UpdatePause();
        }

        /// <summary>Closes the settings panel and its confirm dialog, if open, and resumes the game.</summary>
        private void CloseSettings()
        {
            _confirm.Close();
            CloseSettingsPanel();
            UpdatePause();
        }

        private void UpdatePause()
        {
            _view.SetPaused(_settings != null || _confirm.IsOpen);
        }

        private void CloseSettingsPanel()
        {
            if (_settings == null)
                return;

            _settings.CloseRequestEvent -= OnSettingsCloseRequested;
            _settings.RestartRequestEvent -= OnSettingsRestartRequested;
            _settings.QuitRequestEvent -= OnSettingsQuitRequested;
            _context.Screens.DestroyPanel(_settings);
            _settings = null;
        }

        #endregion
    }
}
