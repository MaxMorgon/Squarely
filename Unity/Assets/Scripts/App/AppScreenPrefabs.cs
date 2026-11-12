using System;
using Squarely.Presentation;
using UnityEngine;

namespace Squarely.App
{
    /// <summary>
    /// The screen prefabs the app states create.
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

        public MenuScreen Menu => _menu;

        public GameView Game => _game;

        public GameOverScreen GameOver => _gameOver;

        /// <summary>The settings panel, over the menu or the game.</summary>
        public SettingsPanel Settings => _settings;

        /// <summary>The "are you sure?" dialog.</summary>
        public ConfirmDialog Confirm => _confirm;

        public TestScreen Test => _test;
    }
}
