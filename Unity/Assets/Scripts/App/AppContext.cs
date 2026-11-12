using System;
using Squarely.Presentation;
using Squarely.Simulation;
using UnityEngine;
using Object = UnityEngine.Object;

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

        /// <summary>Puts replay codes on the clipboard.</summary>
        public ReplayCopier ReplayCopier { get; }

        /// <summary>Picks the seed of every new game.</summary>
        public SeedSource SeedSource { get; }

        /// <summary>The seed the test screen plays.</summary>
        public TestSeed TestSeed { get; }

        /// <summary>Whether the app opens the test screen at start. Ignored when dev tools are off.</summary>
        public StartInTest StartInTest { get; }

        /// <summary>The "Dev tools" setting. Dev tools run only in builds that have them, and while this is on.</summary>
        public DevToolsPreference DevTools { get; }

        /// <summary>Plays sound cues. Lives in the scene, so it survives screen changes.</summary>
        public SoundPlayer Sound { get; }

        /// <summary>Chooses the music of each screen.</summary>
        public MusicDirector Music { get; }

        /// <summary>The sound effects and music toggles of the player.</summary>
        public SoundPreferences SoundPrefs { get; }

        public AppContext(AppScreenPrefabs screenPrefabs, Transform screenRoot, Camera camera, SimConfig defaultConfig, TextsConfig texts,
            HighScore highScore, ReplayHistory replayHistory, ReplayCopier replayCopier, SeedSource seedSource, TestSeed testSeed, StartInTest startInTest,
            DevToolsPreference devTools, SoundPlayer sound, MusicDirector music, SoundPreferences soundPrefs)
        {
            ScreenPrefabs = screenPrefabs;
            ScreenRoot = screenRoot;
            Camera = camera;
            DefaultConfig = defaultConfig;
            Texts = texts;
            HighScore = highScore;
            ReplayHistory = replayHistory;
            ReplayCopier = replayCopier;
            SeedSource = seedSource;
            TestSeed = testSeed;
            StartInTest = startInTest;
            DevTools = devTools;
            Sound = sound;
            Music = music;
            SoundPrefs = soundPrefs;
        }

        /// <summary>
        /// Creates a screen from the prefab under <see cref="ScreenRoot"/>, with its canvas set to render
        /// through <see cref="Camera"/>. The prefab needs a Canvas on the same object.
        /// </summary>
        public T CreateScreen<T>(T prefab) where T : Component
        {
            T screen = Object.Instantiate(prefab, ScreenRoot);
            screen.GetComponent<Canvas>().worldCamera = Camera;
            ButtonSound.Bind(screen.gameObject, Sound);
            return screen;
        }

        /// <summary>Creates the settings panel with its toggles set, and plays the panel-open sound.</summary>
        public SettingsPanel CreateSettings(bool isInGame)
        {
            SettingsPanel panel = CreatePanel(ScreenPrefabs.Settings);
            panel.Initialize(Texts.Settings, isInGame);
            panel.SetSound(SoundPrefs.SfxOn, SoundPrefs.MusicOn);

            if (DevToolsPreference.IsCompiledIn)
                panel.SetDevTools(DevTools.IsOn);

            return panel;
        }

        /// <summary>Like <see cref="CreateScreen{T}"/>, and plays the panel-open sound. For panels and dialogs over a screen.</summary>
        public T CreatePanel<T>(T prefab) where T : Component
        {
            T panel = CreateScreen(prefab);
            PlayHook(hooks => hooks.PanelOpen);
            return panel;
        }

        /// <summary>Destroys a panel made by <see cref="CreatePanel{T}"/> and plays the panel-close sound.</summary>
        public void DestroyPanel(Component panel)
        {
            Object.Destroy(panel.gameObject);
            PlayHook(hooks => hooks.PanelClose);
        }

        /// <summary>Turns sound effects on or off, saves it and plays the toggle sound.</summary>
        public void SetSfx(bool isOn)
        {
            SoundPrefs.SetSfx(isOn);
            PlayHook(hooks => isOn ? hooks.ToggleOn : hooks.ToggleOff);
        }

        /// <summary>Turns music on or off, saves it and plays the toggle sound.</summary>
        public void SetMusic(bool isOn)
        {
            SoundPrefs.SetMusic(isOn);
            PlayHook(hooks => isOn ? hooks.ToggleOn : hooks.ToggleOff);
        }

        /// <summary>Turns dev tools on or off, saves it and plays the toggle sound.</summary>
        public void SetDevTools(bool isOn)
        {
            DevTools.Set(isOn);
            PlayHook(hooks => isOn ? hooks.ToggleOn : hooks.ToggleOff);
        }

        /// <summary>Plays one of the hook cues. Does nothing without a sound player or hooks.</summary>
        public void PlayHook(Func<SoundHooks, SoundCue> pickCue)
        {
            Sound?.PlayHook(pickCue);
        }
    }
}
