using Squarely.Presentation;
using UnityEngine;

namespace Squarely.App
{
    /// <summary>
    /// Shows the main menu and the high score. Play starts the default game; Settings opens the settings panel.
    /// In editor and development builds, Test opens the test screen; other builds hide it.
    /// </summary>
    public sealed class MenuState : IAppState
    {
        private readonly AppContext _context;
        private readonly AppStateMachine _stateMachine;

        private MenuScreen _screen;
        private SettingsPanel _settings;

        public MenuState(AppContext context, AppStateMachine stateMachine)
        {
            _context = context;
            _stateMachine = stateMachine;
        }

        public void Enter()
        {
            _screen = _context.CreateScreen(_context.ScreenPrefabs.Menu);
            _screen.Initialize(_context.Texts.Menu);
            _screen.SetHighScore(_context.HighScore.Best);
            _screen.PlayRequestEvent += OnPlayRequested;
            _screen.SettingsRequestEvent += OnSettingsRequested;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            _screen.TestRequestEvent += OnTestRequested;
#else
            _screen.HideTestButton();
#endif
        }

        public void Exit()
        {
            CloseSettings();

            _screen.PlayRequestEvent -= OnPlayRequested;
            _screen.SettingsRequestEvent -= OnSettingsRequested;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            _screen.TestRequestEvent -= OnTestRequested;
#endif
            Object.Destroy(_screen.gameObject);
        }

        private void OnPlayRequested()
        {
            _stateMachine.ChangeState(new GameState(_context, _stateMachine, _context.DefaultConfig, null, _context.HighScore, () => new MenuState(_context, _stateMachine)));
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private void OnTestRequested()
        {
            _stateMachine.ChangeState(new TestState(_context, _stateMachine));
        }
#endif

        #region Settings

        private void OnSettingsRequested()
        {
            if (_settings != null)
                return;

            _settings = _context.CreateScreen(_context.ScreenPrefabs.Settings);
            _settings.Initialize(_context.Texts.Settings, false);
            _settings.CloseRequestEvent += OnSettingsCloseRequested;
        }

        private void OnSettingsCloseRequested()
        {
            CloseSettings();
        }

        private void CloseSettings()
        {
            if (_settings == null)
                return;

            _settings.CloseRequestEvent -= OnSettingsCloseRequested;
            Object.Destroy(_settings.gameObject);
            _settings = null;
        }

        #endregion
    }
}
