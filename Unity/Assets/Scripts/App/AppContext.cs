using Squarely.Presentation;
using Squarely.Simulation;

namespace Squarely.App
{
    /// <summary>
    /// What the states work with, grouped by area. Built once by the app's entry point and passed to every state.
    /// It has no behaviour.
    /// </summary>
    public sealed class AppContext
    {
        /// <summary>Saved values, seeds and replays.</summary>
        public AppServices Services { get; }

        /// <summary>Sound player, music and sound settings.</summary>
        public AppAudio Audio { get; }

        /// <summary>Creates and destroys screens and panels.</summary>
        public ScreenFactory Screens { get; }

        /// <summary>The game mode the menu's Play button starts.</summary>
        public SimConfig DefaultConfig { get; }

        /// <summary>Every text the screens show.</summary>
        public TextsConfig Texts { get; }

        public AppContext(AppServices services, AppAudio audio, ScreenFactory screens, SimConfig defaultConfig, TextsConfig texts)
        {
            Services = services;
            Audio = audio;
            Screens = screens;
            DefaultConfig = defaultConfig;
            Texts = texts;
        }
    }
}
