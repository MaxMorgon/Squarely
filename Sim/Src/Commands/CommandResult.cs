using System;
using System.Collections.Generic;

namespace Squarely.Simulation
{
    /// <summary>
    /// The outcome of a command. A failed command changed no state and has no events.
    /// </summary>
    public sealed class CommandResult
    {
        private static readonly IReadOnlyList<ISimEvent> NoEvents = Array.Empty<ISimEvent>();

        public bool Success { get; }

        public string FailureReason { get; }

        public IReadOnlyList<ISimEvent> Events { get; }

        private CommandResult(bool success, string failureReason, IReadOnlyList<ISimEvent> events)
        {
            Success = success;
            FailureReason = failureReason;
            Events = events;
        }

        public static CommandResult Succeeded(IReadOnlyList<ISimEvent> events)
        {
            return new CommandResult(true, null, events);
        }

        public static CommandResult Failed(string reason)
        {
            return new CommandResult(false, reason, NoEvents);
        }
    }
}
