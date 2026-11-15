using Squarely.Presentation;
using UnityEngine;

namespace Squarely.App
{
    /// <summary>
    /// Shows the main menu and the high score. Play starts the default game; Settings opens the settings panel.
    /// In the editor and in builds with dev tools, Test opens the test screen while the "Dev tools" setting is on.
    /// Otherwise the button is hidden. The button follows the setting at once.
    /// The menu settings panel also has a Reset progress button, under the same conditions. It closes the settings panel and asks first, then clears the player progress and the best score shown.
    /// </summary>
    public sealed class MenuState : IAppState
    {
        private readonly AppContext _context;
        private readonly AppStateMachine _stateMachine;

        private MenuScreen _screen;
        private SettingsPanel _settings;
#if UNITY_EDITOR || SQUARELY_DEV_TOOLS
        private ConfirmPrompt _confirm;
#endif

        public MenuState(AppContext context, AppStateMachine stateMachine)
        {
            _context = context;
            _stateMachine = stateMachine;
#if UNITY_EDITOR || SQUARELY_DEV_TOOLS
            _confirm = new ConfirmPrompt(context.Screens, context.Texts.Confirm);
#endif
        }

        public void Enter()
        {
            _context.Audio.Music.PlayMenu();
            _screen = _context.Screens.CreateScreen(_context.Screens.Prefabs.Menu);
            _screen.Initialize(_context.Texts.Menu);
            _screen.SetHighScore(_context.Services.HighScore.Best);
            _screen.PlayRequestEvent += OnPlayRequested;
            _screen.SettingsRequestEvent += OnSettingsRequested;
#if UNITY_EDITOR || SQUARELY_DEV_TOOLS
            _screen.TestRequestEvent += OnTestRequested;
            _context.Services.DevTools.ChangeEvent += OnDevToolsChanged;
#endif
            UpdateTestButton();
        }

        public void Exit()
        {
#if UNITY_EDITOR || SQUARELY_DEV_TOOLS
            _confirm.Close();
#endif
            CloseSettings();

            _screen.PlayRequestEvent -= OnPlayRequested;
            _screen.SettingsRequestEvent -= OnSettingsRequested;
#if UNITY_EDITOR || SQUARELY_DEV_TOOLS
            _screen.TestRequestEvent -= OnTestRequested;
            _context.Services.DevTools.ChangeEvent -= OnDevToolsChanged;
#endif
            Object.Destroy(_screen.gameObject);
        }

        private void OnPlayRequested()
        {
            _stateMachine.ChangeState(new GameState(_context, _stateMachine, _context.DefaultConfig, null, _context.Services.HighScore, () => new MenuState(_context, _stateMachine)));
        }

#if UNITY_EDITOR || SQUARELY_DEV_TOOLS
        private void OnTestRequested()
        {
            _stateMachine.ChangeState(new TestState(_context, _stateMachine));
        }

        private void OnDevToolsChanged(bool isOn)
        {
            UpdateTestButton();
            UpdateResetProgressButton();
        }
#endif

        private void UpdateTestButton()
        {
            if (_context.Services.DevTools.IsActive)
                _screen.ShowTestButton();
            else
                _screen.HideTestButton();
        }

        #region Reset progress

#if UNITY_EDITOR || SQUARELY_DEV_TOOLS
        private void UpdateResetProgressButton()
        {
            if (_settings != null)
                _settings.SetResetProgressVisible(_context.Services.DevTools.IsActive);
        }

        private void OnResetProgressRequested()
        {
            if (_confirm.IsOpen)
                return;

            // The question shows over the menu, not over the settings panel.
            CloseSettings();

            _confirm.Ask(_context.Texts.Confirm.ResetProgressQuestion, OnResetProgressConfirmed);
        }

        private void OnResetProgressConfirmed()
        {
            _context.Services.Progress.Reset();
            _screen.SetHighScore(_context.Services.HighScore.Best);
        }
#endif

        #endregion

        #region Settings

        private void OnSettingsRequested()
        {
            if (_settings != null)
                return;

            _settings = _context.Screens.CreateSettings(isInGame: false, showDevTools: true);
            _settings.CloseRequestEvent += OnSettingsCloseRequested;
#if UNITY_EDITOR || SQUARELY_DEV_TOOLS
            _settings.ResetProgressRequestEvent += OnResetProgressRequested;
            UpdateResetProgressButton();
#endif
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
#if UNITY_EDITOR || SQUARELY_DEV_TOOLS
            _settings.ResetProgressRequestEvent -= OnResetProgressRequested;
#endif
            _context.Screens.DestroyPanel(_settings);
            _settings = null;
        }

        #endregion
    }
}
