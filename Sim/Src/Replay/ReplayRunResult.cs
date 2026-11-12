namespace Squarely.Simulation
{
    /// <summary>What happened when a replay ran against a config. See <see cref="ReplayRunner"/>.</summary>
    public sealed class ReplayRunResult
    {
        /// <summary>False if the config differs from the one the replay was recorded with.</summary>
        public bool FingerprintMatches { get; }

        /// <summary>Index of the first command that failed, or -1 if all succeeded. Later commands did not run.</summary>
        public int FirstFailedIndex { get; }

        public string FailureReason { get; }

        public int EndScore { get; }

        public bool EndIsResolved { get; }

        /// <summary>True if the end score and resolved flag equal the recorded ones.</summary>
        public bool EndStateMatches { get; }

        public bool AllCommandsSucceeded => FirstFailedIndex < 0;

        public ReplayRunResult(
            bool fingerprintMatches,
            int firstFailedIndex,
            string failureReason,
            int endScore,
            bool endIsResolved,
            bool endStateMatches)
        {
            FingerprintMatches = fingerprintMatches;
            FirstFailedIndex = firstFailedIndex;
            FailureReason = failureReason;
            EndScore = endScore;
            EndIsResolved = endIsResolved;
            EndStateMatches = endStateMatches;
        }
    }
}
