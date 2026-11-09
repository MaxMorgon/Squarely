using System;
using Squarely.Presentation;
using UnityEngine;

namespace Squarely.App
{
    /// <summary>
    /// The screen prefabs the app states create.
    /// </summary>
    [Serializable]
    public sealed class AppScreens
    {
        [SerializeField] private MenuScreen _menu;
        [SerializeField] private GameController _game;

        public MenuScreen Menu => _menu;

        public GameController Game => _game;
    }
}
