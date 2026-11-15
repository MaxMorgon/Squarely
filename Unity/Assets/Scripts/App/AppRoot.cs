using Squarely.Presentation;
using UnityEngine;
using UnityEngine.Audio;

namespace Squarely.App
{
    /// <summary>
    /// The app's entry point and the only object in the scene that starts anything.
    /// Builds the app context from its serialized setup and opens the menu, or in the editor and in builds with dev tools
    /// the test screen if "start in test screen" and dev tools are both on.
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

        [Header("Audio")]
        [Tooltip("Plays the sounds. Lives in the scene, so it survives screen changes. Its Sound Hooks hold the UI cues.")]
        [SerializeField] private SoundPlayer _soundPlayer;

        [SerializeField] private MusicPlayer _musicPlayer;

        [Tooltip("The menu and game tracks and the fade values.")]
        [SerializeField] private MusicSetup _musicSetup;

        [Tooltip("The mixer with the exposed parameters SfxVolume and MusicVolume.")]
        [SerializeField] private AudioMixer _mixer;

        private AppStateMachine _stateMachine;

        #region Unity

        private void Start()
        {
            var store = new PlayerPrefsStore();
            var highScore = new HighScore(store);
            var replayHistory = new ReplayHistory(store, () => System.DateTime.UtcNow);
            var seedSource = new SeedSource(new System.Random());
#if UNITY_EDITOR || SQUARELY_DEV_TOOLS
            var testSeed = new TestSeed(store, seedSource);
            var startInTest = new StartInTest(store);
#endif
            var devTools = new DevToolsPreference(store);
            var soundPreferences = new SoundPreferences(store, new SoundSettings(_mixer));
            _soundPlayer.SetHaptics(new NoHaptics());
            var music = new MusicDirector(_musicPlayer, _musicSetup);
            var services = new AppServices(highScore, new PlayerProgress(highScore), replayHistory, new ReplayCopier(new SystemClipboard()), seedSource, devTools
#if UNITY_EDITOR || SQUARELY_DEV_TOOLS
                , testSeed, startInTest
#endif
                );
            var audio = new AppAudio(_soundPlayer, music, soundPreferences);
            var screens = new ScreenFactory(_screenPrefabs, transform, _camera, _texts, audio, devTools);
            var context = new AppContext(services, audio, screens, _defaultMode.Config, _texts);

            _stateMachine = new AppStateMachine();
            _stateMachine.ChangeState(CreateFirstState(context));
        }

        private void Update()
        {
            _stateMachine.Tick(Time.deltaTime);
        }

        #endregion

        private IAppState CreateFirstState(AppContext context)
        {
#if UNITY_EDITOR || SQUARELY_DEV_TOOLS
            if (context.Services.DevTools.IsActive && context.Services.StartInTest.IsOn)
                return new TestState(context, _stateMachine);
#endif

            return new MenuState(context, _stateMachine);
        }
    }
}
