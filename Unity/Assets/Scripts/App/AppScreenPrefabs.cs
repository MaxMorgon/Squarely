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

        public MenuScreen Menu => _menu;

        public GameView Game => _game;

        public GameOverScreen GameOver => _gameOver;
    }
}
