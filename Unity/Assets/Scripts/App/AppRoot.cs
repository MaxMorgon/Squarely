using Squarely.Presentation;
using UnityEngine;

namespace Squarely.App
{
    /// <summary>
    /// The app's entry point and the only object in the scene that starts anything.
    /// Builds the app context from its serialized setup and opens the menu.
    /// </summary>
    public sealed class AppRoot : MonoBehaviour
    {
        [SerializeField] private AppScreenPrefabs _screenPrefabs;

        [Tooltip("The camera that renders the game.")]
        [SerializeField] private Camera _camera;

        [Tooltip("The game mode started from the menu. Edits apply live, also in play mode. Board and hand size changes need a play mode restart.")]
        [SerializeField] private SimConfigAsset _defaultMode;

        [Tooltip("0 picks a new seed for every game.")]
        [SerializeField] private int _seed;

        private AppStateMachine _stateMachine;

        #region Unity

        private void Start()
        {
            ulong? seed = _seed != 0 ? (ulong)_seed : (ulong?)null;
            var defaultGameSetup = new GameSetup(_defaultMode.Config, seed);
            var context = new AppContext(_screenPrefabs, transform, _camera, defaultGameSetup);

            _stateMachine = new AppStateMachine();
            _stateMachine.ChangeState(new MenuState(context, _stateMachine));
        }

        #endregion
    }
}
