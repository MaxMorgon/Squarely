using Squarely.Presentation;
using UnityEngine;

namespace Squarely.App
{
    /// <summary>
    /// Shows the main menu and the high score. Play starts the default game.
    /// In editor and development builds, Test opens the test screen; other builds hide it.
    /// </summary>
    public sealed class MenuState : IAppState
    {
        private readonly AppContext _context;
        private readonly AppStateMachine _stateMachine;

        private MenuScreen _screen;

        public MenuState(AppContext context, AppStateMachine stateMachine)
        {
            _context = context;
            _stateMachine = stateMachine;
        }

        public void Enter()
        {
            _screen = Object.Instantiate(_context.ScreenPrefabs.Menu, _context.ScreenRoot);
            _screen.Initialize(_context.Texts.Menu);
            _screen.SetHighScore(_context.HighScore.Best);
            _screen.PlayRequestEvent += OnPlayRequested;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            _screen.TestRequestEvent += OnTestRequested;
#else
            _screen.HideTestButton();
#endif
        }

        public void Exit()
        {
            _screen.PlayRequestEvent -= OnPlayRequested;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            _screen.TestRequestEvent -= OnTestRequested;
#endif
            Object.Destroy(_screen.gameObject);
        }

        private void OnPlayRequested()
        {
            _stateMachine.ChangeState(new GameState(_context, _stateMachine, _context.DefaultGameSetup));
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private void OnTestRequested()
        {
            _stateMachine.ChangeState(new TestState(_context, _stateMachine));
        }
#endif
    }
}
