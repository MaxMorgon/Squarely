using Squarely.Presentation;
using UnityEngine;

namespace Squarely.App
{
    /// <summary>
    /// Shows the main menu. Play starts the default game.
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
            _screen.PlayRequested += OnPlayRequested;
        }

        public void Exit()
        {
            _screen.PlayRequested -= OnPlayRequested;
            Object.Destroy(_screen.gameObject);
        }

        private void OnPlayRequested()
        {
            _stateMachine.ChangeState(new GameState(_context, _stateMachine, _context.DefaultGameSetup));
        }
    }
}
