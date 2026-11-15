using System;
using System.Collections.Generic;

namespace Squarely.Simulation
{
    /// <summary>
    /// Records a game while it plays. Run every command through <see cref="Execute"/>;
    /// only successful commands are kept. <see cref="Build"/> gives the replay so far at any time.
    /// Create it for a sim that has had no commands yet.
    /// </summary>
    public sealed class ReplayRecorder
    {
        private Sim _sim;
        private readonly List<ISimCommand> _commands = new List<ISimCommand>();

        internal int CommandCount => _commands.Count;

        public ReplayRecorder(Sim sim)
        {
            _sim = sim ?? throw new ArgumentNullException(nameof(sim));
        }

        /// <summary>Runs the command on the sim and records it if it succeeded.</summary>
        public CommandResult Execute(ISimCommand command)
        {
            CommandResult result = _sim.Execute(command);

            if (result.Success)
                _commands.Add(command);

            return result;
        }

        /// <summary>
        /// Continues the recording on another sim, for a game stepped back to an earlier state.
        /// Keeps the first <paramref name="commandCount"/> commands and drops the rest.
        /// The sim must be the state after exactly those commands.
        /// </summary>
        public void Restore(Sim sim, int commandCount)
        {
            if (sim == null)
                throw new ArgumentNullException(nameof(sim));

            if (commandCount < 0 || commandCount > _commands.Count)
                throw new ArgumentOutOfRangeException(nameof(commandCount));

            _sim = sim;
            _commands.RemoveRange(commandCount, _commands.Count - commandCount);
        }

        /// <summary>The replay of the commands so far, with the sim's current score and resolved flag.</summary>
        public GameReplay Build()
        {
            return new GameReplay(_sim.Seed, _sim.ConfigFingerprint, _commands, _sim.Score, _sim.IsResolved);
        }
    }
}
