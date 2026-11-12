using Squarely.Simulation;

namespace Squarely.App
{
    /// <summary>The result of <see cref="ReplayPaste.Check"/>.</summary>
    public sealed class ReplayPasteResult
    {
        public ReplayPasteStatus Status { get; }

        /// <summary>The decoded replay. Null unless the status is Ok or ConfigMismatch.</summary>
        public GameReplay Replay { get; }

        /// <summary>Why the code is not valid. Null unless the status is Invalid.</summary>
        public string Error { get; }

        /// <summary>Whether the replay can be played: it decoded, with or without a config mismatch.</summary>
        public bool CanPlay => Replay != null;

        public ReplayPasteResult(ReplayPasteStatus status, GameReplay replay, string error)
        {
            Status = status;
            Replay = replay;
            Error = error;
        }
    }
}
