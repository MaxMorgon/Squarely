using Squarely.Presentation;
using UnityEngine;

namespace Squarely.App
{
    /// <summary>
    /// The scene objects and app setup the states work with. Built once by the app's entry point
    /// and passed to every state.
    /// </summary>
    public sealed class AppContext
    {
        /// <summary>Prefabs of the screens the states create.</summary>
        public AppScreenPrefabs ScreenPrefabs { get; }

        /// <summary>Parent of every screen the states create.</summary>
        public Transform ScreenRoot { get; }

        /// <summary>The camera that renders the game.</summary>
        public Camera Camera { get; }

        /// <summary>The game started from the menu's Play button.</summary>
        public GameSetup DefaultGameSetup { get; }

        /// <summary>Every text the screens show.</summary>
        public TextsConfig Texts { get; }

        /// <summary>The best score on this device.</summary>
        public HighScore HighScore { get; }

        public AppContext(AppScreenPrefabs screenPrefabs, Transform screenRoot, Camera camera, GameSetup defaultGameSetup, TextsConfig texts,
            HighScore highScore)
        {
            ScreenPrefabs = screenPrefabs;
            ScreenRoot = screenRoot;
            Camera = camera;
            DefaultGameSetup = defaultGameSetup;
            Texts = texts;
            HighScore = highScore;
        }
    }
}
