using Squarely.Presentation;
using Squarely.Simulation;
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

        /// <summary>The game mode the menu's Play button starts.</summary>
        public SimConfig DefaultConfig { get; }

        /// <summary>Every text the screens show.</summary>
        public TextsConfig Texts { get; }

        /// <summary>The best score on this device.</summary>
        public HighScore HighScore { get; }

        /// <summary>The last games played on this device.</summary>
        public ReplayHistory ReplayHistory { get; }

        /// <summary>Picks the seed of every new game.</summary>
        public SeedSource SeedSource { get; }

        /// <summary>The seed the test screen plays.</summary>
        public TestSeed TestSeed { get; }

        /// <summary>Whether the app opens the test screen at start. Editor and development builds only.</summary>
        public StartInTest StartInTest { get; }

        public AppContext(AppScreenPrefabs screenPrefabs, Transform screenRoot, Camera camera, SimConfig defaultConfig, TextsConfig texts,
            HighScore highScore, ReplayHistory replayHistory, SeedSource seedSource, TestSeed testSeed, StartInTest startInTest)
        {
            ScreenPrefabs = screenPrefabs;
            ScreenRoot = screenRoot;
            Camera = camera;
            DefaultConfig = defaultConfig;
            Texts = texts;
            HighScore = highScore;
            ReplayHistory = replayHistory;
            SeedSource = seedSource;
            TestSeed = testSeed;
            StartInTest = startInTest;
        }

        /// <summary>
        /// Creates a screen from the prefab under <see cref="ScreenRoot"/>, with its canvas set to render
        /// through <see cref="Camera"/>. The prefab needs a Canvas on the same object.
        /// </summary>
        public T CreateScreen<T>(T prefab) where T : Component
        {
            T screen = Object.Instantiate(prefab, ScreenRoot);
            screen.GetComponent<Canvas>().worldCamera = Camera;
            return screen;
        }
    }
}
