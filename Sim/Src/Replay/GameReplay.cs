using System;
using System.Collections.Generic;

namespace Squarely.Simulation
{
    /// <summary>
    /// A recorded game: the seed, the config fingerprint, the successful commands in order,
    /// and the score and resolved flag after the last one. Can be a finished or a half-finished game.
    /// Immutable.
    /// </summary>
    public sealed class GameReplay
    {
        public const int CurrentVersion = 1;

        public int FormatVersion { get; }

        public ulong Seed { get; }

        /// <summary>See <see cref="Simulation.ConfigFingerprint"/>.</summary>
        public ulong ConfigFingerprint { get; }

        public IReadOnlyList<ISimCommand> Commands { get; }

        public int EndScore { get; }

        public bool EndIsResolved { get; }

        public GameReplay(
            ulong seed,
            ulong configFingerprint,
            IEnumerable<ISimCommand> commands,
            int endScore,
            bool endIsResolved,
            int formatVersion = CurrentVersion)
        {
            if (commands == null)
                throw new ArgumentNullException(nameof(commands));

            FormatVersion = formatVersion;
            Seed = seed;
            ConfigFingerprint = configFingerprint;
            Commands = new List<ISimCommand>(commands);
            EndScore = endScore;
            EndIsResolved = endIsResolved;
        }
    }
}
