using Squarely.Presentation;
using UnityEngine;

namespace Squarely.App
{
    /// <summary>
    /// The app's entry point and the only object in the scene that starts anything.
    /// Builds the app flow from its serialized setup and opens the menu.
    /// </summary>
    public sealed class AppRoot : MonoBehaviour
    {
        [SerializeField] private AppScreens _screens;

        [Tooltip("The camera that renders the game.")]
        [SerializeField] private Camera _camera;

        [Tooltip("The game mode started from the menu. Edits apply live, also in play mode. Board and hand size changes need a play mode restart.")]
        [SerializeField] private SimConfigAsset _defaultMode;

        [Tooltip("0 picks a new seed for every game.")]
        [SerializeField] private int _seed;

        private AppFlow _flow;

        #region Unity

        private void Start()
        {
            ulong? seed = _seed != 0 ? (ulong)_seed : (ulong?)null;
            var defaultGameSetup = new GameSetup(_defaultMode.Config, seed);

            _flow = new AppFlow(_screens, transform, _camera, defaultGameSetup);
            _flow.GoToMenu();
        }

        #endregion
    }
}
