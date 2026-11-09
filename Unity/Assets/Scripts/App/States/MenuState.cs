using Squarely.Presentation;
using UnityEngine;

namespace Squarely.App
{
    /// <summary>
    /// Shows the main menu. Play starts the default game.
    /// </summary>
    public sealed class MenuState : IAppState
    {
        private readonly AppFlow _flow;
        private readonly MenuScreen _screenPrefab;
        private readonly Transform _screenRoot;

        private MenuScreen _screen;

        public MenuState(AppFlow flow, MenuScreen screenPrefab, Transform screenRoot)
        {
            _flow = flow;
            _screenPrefab = screenPrefab;
            _screenRoot = screenRoot;
        }

        public void Enter()
        {
            _screen = Object.Instantiate(_screenPrefab, _screenRoot);
            _screen.PlayRequested += OnPlayRequested;
        }

        public void Exit()
        {
            _screen.PlayRequested -= OnPlayRequested;
            Object.Destroy(_screen.gameObject);
        }

        private void OnPlayRequested()
        {
            _flow.GoToGame(_flow.DefaultGameSetup);
        }
    }
}
