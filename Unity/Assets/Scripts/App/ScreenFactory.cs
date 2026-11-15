using Squarely.Presentation;
using Squarely.Simulation;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Squarely.App
{
    /// <summary>
    /// Creates and destroys the screens, panels and game view for the states, set up the same way every time.
    /// </summary>
    public sealed class ScreenFactory
    {
        private readonly Transform _root;
        private readonly Camera _camera;
        private readonly TextsConfig _texts;
        private readonly AppAudio _audio;
        private readonly DevToolsPreference _devTools;

        /// <summary>Prefabs of the screens the states create.</summary>
        public AppScreenPrefabs Prefabs { get; }

        /// <param name="prefabs">Prefabs of the screens.</param>
        /// <param name="root">Parent of every screen created.</param>
        /// <param name="camera">The camera that renders the game and the screens.</param>
        public ScreenFactory(AppScreenPrefabs prefabs, Transform root, Camera camera, TextsConfig texts, AppAudio audio, DevToolsPreference devTools)
        {
            Prefabs = prefabs;
            _root = root;
            _camera = camera;
            _texts = texts;
            _audio = audio;
            _devTools = devTools;
        }

        /// <summary>
        /// Creates a screen from the prefab under the root, with its canvas set to render through the camera.
        /// Its <see cref="ButtonSound"/> buttons play the button sound, also buttons made later.
        /// The prefab needs a Canvas on the same object.
        /// </summary>
        public T CreateScreen<T>(T prefab) where T : Component
        {
            T screen = Object.Instantiate(prefab, _root);
            screen.GetComponent<Canvas>().worldCamera = _camera;
            ButtonSoundScope.Attach(screen.gameObject, _audio.Sound);
            return screen;
        }

        /// <summary>
        /// Starts the game music and creates the game view and its controller for the setup, connected to each other.
        /// The caller starts the game and destroys the view.
        /// </summary>
        public (GameView View, GameController Controller) CreateGame(GameSetup setup)
        {
            _audio.Music.PlayGame();
            GameView view = Object.Instantiate(Prefabs.Game, _root);
            var controller = new GameController(view, setup);
            view.Initialize(setup.Config, _camera, _texts.GameHud, controller, _audio.Sound);
            return (view, controller);
        }

        /// <summary>
        /// Creates the settings panel with its toggles set, and plays the panel-open sound.
        /// The sound effects, music and dev tools toggles save their setting and play the toggle sound.
        /// The dev tools toggle shows only if <paramref name="showDevTools"/> is true and the build has dev tools.
        /// Destroy the panel with <see cref="DestroyPanel"/>.
        /// </summary>
        public SettingsPanel CreateSettings(bool isInGame, bool showDevTools)
        {
            SettingsPanel panel = CreatePanel(Prefabs.Settings);
            panel.Initialize(_texts.Settings, isInGame);
            panel.SetSound(_audio.Prefs.SfxOn, _audio.Prefs.MusicOn);
            panel.SfxChangeEvent += _audio.SetSfx;
            panel.MusicChangeEvent += _audio.SetMusic;

            if (showDevTools && DevToolsPreference.IsCompiledIn)
            {
                panel.SetDevTools(_devTools.IsOn);
                panel.DevToolsChangeEvent += OnDevToolsChanged;
            }

            return panel;
        }

        /// <summary>Like <see cref="CreateScreen{T}"/>, and plays the panel-open sound. For panels and dialogs over a screen.</summary>
        public T CreatePanel<T>(T prefab) where T : Component
        {
            T panel = CreateScreen(prefab);
            _audio.Sound.PlayHook(hooks => hooks.PanelOpen);
            return panel;
        }

        /// <summary>Destroys a panel made by <see cref="CreatePanel{T}"/> or <see cref="CreateSettings"/> and plays the panel-close sound.</summary>
        public void DestroyPanel(Component panel)
        {
            Object.Destroy(panel.gameObject);
            _audio.Sound.PlayHook(hooks => hooks.PanelClose);
        }

        private void OnDevToolsChanged(bool isOn)
        {
            _devTools.Set(isOn);
            _audio.PlayToggle(isOn);
        }
    }
}
