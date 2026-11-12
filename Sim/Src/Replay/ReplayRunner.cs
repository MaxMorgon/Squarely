using System;

namespace Squarely.Simulation
{
    /// <summary>
    /// Runs replays. To step through one, call <see cref="CreateSim"/> and run
    /// <see cref="GameReplay.Commands"/> one by one with <see cref="Sim.Execute"/>.
    /// </summary>
    public static class ReplayRunner
    {
        /// <summary>A new sim at the start of the replay: its seed and the given config.</summary>
        public static Sim CreateSim(GameReplay replay, SimConfig config)
        {
            return CreateSim(replay, config, RuleFactory.CreateDefault());
        }

        public static Sim CreateSim(GameReplay replay, SimConfig config, RuleFactory rules)
        {
            if (replay == null)
                throw new ArgumentNullException(nameof(replay));

            return new Sim(config, replay.Seed, rules);
        }

        /// <summary>
        /// Runs all commands against the config, stopping at the first failure. It runs even if the config
        /// differs from the recorded one: the result then reports the mismatch.
        /// </summary>
        public static ReplayRunResult Run(GameReplay replay, SimConfig config)
        {
            return Run(replay, config, RuleFactory.CreateDefault());
        }

        public static ReplayRunResult Run(GameReplay replay, SimConfig config, RuleFactory rules)
        {
            Sim sim = CreateSim(replay, config, rules);
            int failedIndex = -1;
            string failureReason = null;

            for (int i = 0; i < replay.Commands.Count; i++)
            {
                CommandResult result = sim.Execute(replay.Commands[i]);

                if (result.Success)
                    continue;

                failedIndex = i;
                failureReason = result.FailureReason;
                break;
            }

            bool endMatches = replay.MatchesEnd(sim);

            return new ReplayRunResult(
                sim.ConfigFingerprint == replay.ConfigFingerprint,
                failedIndex,
                failureReason,
                sim.Score,
                sim.IsResolved,
                endMatches);
        }
    }
}
