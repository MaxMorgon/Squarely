using Squarely.Simulation;

namespace Squarely.Presentation
{
    /// <summary>
    /// Everything needed to start a game: the game mode and the seed.
    /// </summary>
    public sealed class GameSetup
    {
        /// <summary>The game mode. Edits to it apply from the next game started.</summary>
        public SimConfig Config { get; }

        /// <summary>The seed for every game started with this setup, or null for a new seed per game.</summary>
        public ulong? Seed { get; }

        public GameSetup(SimConfig config, ulong? seed)
        {
            Config = config;
            Seed = seed;
        }
    }
}
