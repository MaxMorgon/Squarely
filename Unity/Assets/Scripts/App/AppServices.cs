using Squarely.Presentation;

namespace Squarely.App
{
    /// <summary>
    /// The app's data services: saved values, seeds and replays. Built once by the app's entry point.
    /// </summary>
    public sealed class AppServices
    {
        /// <summary>The best score on this device.</summary>
        public HighScore HighScore { get; }

        /// <summary>Everything the player earned. Reset clears it.</summary>
        public PlayerProgress Progress { get; }

        /// <summary>The last games played on this device.</summary>
        public ReplayHistory ReplayHistory { get; }

        /// <summary>Puts replay codes on the clipboard.</summary>
        public ReplayCopier ReplayCopier { get; }

        /// <summary>Picks the seed of every new game.</summary>
        public SeedSource SeedSource { get; }

        /// <summary>The "Dev tools" setting. Dev tools run only in builds that have them, and while this is on.</summary>
        public DevToolsPreference DevTools { get; }

#if UNITY_EDITOR || SQUARELY_DEV_TOOLS
        /// <summary>The seed the test screen plays.</summary>
        public TestSeed TestSeed { get; }

        /// <summary>Whether the app opens the test screen at start. Ignored when dev tools are off.</summary>
        public StartInTest StartInTest { get; }
#endif

        public AppServices(HighScore highScore, PlayerProgress progress, ReplayHistory replayHistory, ReplayCopier replayCopier, SeedSource seedSource,
            DevToolsPreference devTools
#if UNITY_EDITOR || SQUARELY_DEV_TOOLS
            , TestSeed testSeed, StartInTest startInTest
#endif
            )
        {
            HighScore = highScore;
            Progress = progress;
            ReplayHistory = replayHistory;
            ReplayCopier = replayCopier;
            SeedSource = seedSource;
            DevTools = devTools;
#if UNITY_EDITOR || SQUARELY_DEV_TOOLS
            TestSeed = testSeed;
            StartInTest = startInTest;
#endif
        }
    }
}
