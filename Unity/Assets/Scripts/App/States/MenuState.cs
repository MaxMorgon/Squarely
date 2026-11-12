using Squarely.Presentation;
using UnityEngine;

namespace Squarely.App
{
    /// <summary>
    /// Shows the main menu and the high score. Play starts the default game; Settings opens the settings panel.
    /// In the editor and in builds with dev tools, Test opens the test screen while the "Dev tools" setting is on.
    /// Otherwise the button is hidden. The button follows the setting at once.
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
            _context.Music.PlayMenu();
            _screen = _context.CreateScreen(_context.ScreenPrefabs.Menu);
            _screen.Initialize(_context.Texts.Menu);
            _screen.SetHighScore(_context.HighScore.Best);
            _screen.PlayRequestEvent += OnPlayRequested;
            _screen.SettingsRequestEvent += OnSettingsRequested;
#if UNITY_EDITOR || SQUARELY_DEV_TOOLS
            _screen.TestRequestEvent += OnTestRequested;
            _context.DevTools.ChangeEvent += OnDevToolsChanged;
#endif
            UpdateTestButton();
        }

        public void Exit()
        {
            CloseSettings();

            _screen.PlayRequestEvent -= OnPlayRequested;
            _screen.SettingsRequestEvent -= OnSettingsRequested;
#if UNITY_EDITOR || SQUARELY_DEV_TOOLS
            _screen.TestRequestEvent -= OnTestRequested;
            _context.DevTools.ChangeEvent -= OnDevToolsChanged;
#endif
            Object.Destroy(_screen.gameObject);
        }

        private void OnPlayRequested()
        {
            _stateMachine.ChangeState(new GameState(_context, _stateMachine, _context.DefaultConfig, null, _context.HighScore, () => new MenuState(_context, _stateMachine)));
        }

#if UNITY_EDITOR || SQUARELY_DEV_TOOLS
        private void OnTestRequested()
        {
            _stateMachine.ChangeState(new TestState(_context, _stateMachine));
        }

        private void OnDevToolsChanged(bool isOn)
        {
            UpdateTestButton();
        }
#endif

        private void UpdateTestButton()
        {
            if (_context.DevTools.IsActive)
                _screen.ShowTestButton();
            else
                _screen.HideTestButton();
        }

        #region Settings

        private void OnSettingsRequested()
        {
            if (_settings != null)
                return;

            _settings = _context.CreateSettings(false);
            _settings.CloseRequestEvent += OnSettingsCloseRequested;
            _settings.DevToolsChangeEvent += _context.SetDevTools;
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
            _settings.DevToolsChangeEvent -= _context.SetDevTools;
            _context.DestroyPanel(_settings);
            _settings = null;
        }

        #endregion
    }
}
