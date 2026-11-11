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

        [Tooltip("The game mode started from the menu. Edits apply live, also in play mode. Board and hand size changes apply from the next game.")]
        [SerializeField] private SimConfigAsset _defaultMode;

        [Tooltip("Every text the screens show.")]
        [SerializeField] private TextsConfig _texts;

        [Tooltip("0 picks a new seed for every game.")]
        [SerializeField] private int _seed;

        private AppStateMachine _stateMachine;

        #region Unity

        private void Start()
        {
            ulong? seed = _seed != 0 ? (ulong)_seed : (ulong?)null;
            var defaultGameSetup = new GameSetup(_defaultMode.Config, seed);
            var store = new PlayerPrefsStore();
            var highScore = new HighScore(store);
            var testSeed = new TestSeed(store, new System.Random());
            var context = new AppContext(_screenPrefabs, transform, _camera, defaultGameSetup, _texts, highScore, testSeed);

            _stateMachine = new AppStateMachine();
            _stateMachine.ChangeState(new MenuState(context, _stateMachine));
        }

        #endregion
    }
}
