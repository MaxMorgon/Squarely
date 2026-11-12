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
        private readonly Sim _sim;
        private readonly List<ISimCommand> _commands = new List<ISimCommand>();

        public int CommandCount => _commands.Count;

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

        /// <summary>The replay of the commands so far, with the sim's current score and resolved flag.</summary>
        public GameReplay Build()
        {
            return new GameReplay(_sim.Seed, _sim.ConfigFingerprint, _commands, _sim.Score, _sim.IsResolved);
        }
    }
}
