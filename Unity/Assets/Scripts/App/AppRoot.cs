using Squarely.Presentation;
using UnityEngine;

namespace Squarely.App
{
    /// <summary>
    /// The app's entry point and the only object in the scene that starts anything.
    /// Builds the app context from its serialized setup and opens the menu, or in editor and development builds
    /// the test screen if "start in test screen" is on.
    /// </summary>
    public sealed class AppRoot : MonoBehaviour
    {
        [SerializeField] private AppScreenPrefabs _screenPrefabs;

        [Tooltip("The camera that renders the game.")]
        [SerializeField] private Camera _camera;

        [Tooltip("The game mode started from the menu. Edits, also in play mode, apply from the next game.")]
        [SerializeField] private SimConfigAsset _defaultMode;

        [Tooltip("Every text the screens show.")]
        [SerializeField] private TextsConfig _texts;

        private AppStateMachine _stateMachine;

        #region Unity

        private void Start()
        {
            var defaultGameSetup = new GameSetup(_defaultMode.Config, null);
            var store = new PlayerPrefsStore();
            var highScore = new HighScore(store);
            var testSeed = new TestSeed(store, new System.Random());
            var startInTest = new StartInTest(store);
            var context = new AppContext(_screenPrefabs, transform, _camera, defaultGameSetup, _texts, highScore, testSeed, startInTest);

            _stateMachine = new AppStateMachine();
            _stateMachine.ChangeState(CreateFirstState(context));
        }

        #endregion

        private IAppState CreateFirstState(AppContext context)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (context.StartInTest.IsOn)
                return new TestState(context, _stateMachine);
#endif

            return new MenuState(context, _stateMachine);
        }
    }
}
