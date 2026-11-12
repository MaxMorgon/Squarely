using System;
using Squarely.Presentation;
using UnityEngine;

namespace Squarely.App
{
    /// <summary>
    /// The screen prefabs the app states create. The test and replay fields are serialized in every build. Only builds with dev tools read them.
    /// </summary>
    [Serializable]
    public sealed class AppScreenPrefabs
    {
        [SerializeField] private MenuScreen _menu;
        [SerializeField] private GameView _game;
        [SerializeField] private GameOverScreen _gameOver;
        [SerializeField] private SettingsPanel _settings;
        [SerializeField] private ConfirmDialog _confirm;
        [SerializeField] private TestScreen _test;
        [SerializeField] private ReplayScreen _replay;

        public MenuScreen Menu => _menu;

        public GameView Game => _game;

        public GameOverScreen GameOver => _gameOver;

        /// <summary>The settings panel, over the menu or the game.</summary>
        public SettingsPanel Settings => _settings;

        /// <summary>The "are you sure?" dialog.</summary>
        public ConfirmDialog Confirm => _confirm;
#if UNITY_EDITOR || SQUARELY_DEV_TOOLS

        public TestScreen Test => _test;

        /// <summary>The replay watch screen, shown over the game view.</summary>
        public ReplayScreen Replay => _replay;
#endif
    }
}
