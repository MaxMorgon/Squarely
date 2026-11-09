using Squarely.Presentation;
using UnityEngine;

namespace Squarely.App
{
    /// <summary>
    /// Every place the app can go, in one list. States call these to move on;
    /// each call creates a fresh state with what it needs and makes it current.
    /// </summary>
    public sealed class AppFlow
    {
        private readonly AppStateMachine _stateMachine = new AppStateMachine();
        private readonly AppScreens _screens;
        private readonly Transform _screenRoot;
        private readonly Camera _camera;

        /// <summary>The game started from the menu's Play button.</summary>
        public GameSetup DefaultGameSetup { get; }

        /// <param name="screens">Prefabs of the screens the states create.</param>
        /// <param name="screenRoot">Parent of every screen the states create.</param>
        /// <param name="camera">The camera that renders the game.</param>
        /// <param name="defaultGameSetup">The game started from the menu.</param>
        public AppFlow(AppScreens screens, Transform screenRoot, Camera camera, GameSetup defaultGameSetup)
        {
            _screens = screens;
            _screenRoot = screenRoot;
            _camera = camera;
            DefaultGameSetup = defaultGameSetup;
        }

        public void GoToMenu()
        {
            _stateMachine.ChangeState(new MenuState(this, _screens.Menu, _screenRoot));
        }

        public void GoToGame(GameSetup setup)
        {
            _stateMachine.ChangeState(new GameState(this, _screens, _screenRoot, _camera, setup));
        }
    }
}
