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

        /// <summary>The number of place commands: the moves the player made.</summary>
        public int MoveCount => CountMoves(Commands.Count);

        /// <summary>The number of moves in the first <paramref name="commandCount"/> commands. The one rule for what counts as a move.</summary>
        public int CountMoves(int commandCount)
        {
            if (commandCount < 0 || commandCount > Commands.Count)
                throw new ArgumentOutOfRangeException(nameof(commandCount));

            int count = 0;

            for (int i = 0; i < commandCount; i++)
            {
                if (Commands[i] is PlacePieceCommand)
                    count++;
            }

            return count;
        }

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

        /// <summary>Whether the sim has the recorded end score and resolved flag.</summary>
        public bool MatchesEnd(Sim sim)
        {
            if (sim == null)
                throw new ArgumentNullException(nameof(sim));

            return sim.Score == EndScore && sim.IsResolved == EndIsResolved;
        }
    }
}
