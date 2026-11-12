namespace Squarely.Simulation
{
    /// <summary>The result of <see cref="ReplayCodec.Decode(string)"/>: a replay, or a failure reason.</summary>
    public sealed class ReplayDecodeResult
    {
        public bool Success => Failure == ReplayDecodeFailure.None;

        /// <summary>Null on failure.</summary>
        public GameReplay Replay { get; }

        public ReplayDecodeFailure Failure { get; }

        public string Message { get; }

        private ReplayDecodeResult(GameReplay replay, ReplayDecodeFailure failure, string message)
        {
            Replay = replay;
            Failure = failure;
            Message = message;
        }

        public static ReplayDecodeResult Succeeded(GameReplay replay)
        {
            return new ReplayDecodeResult(replay, ReplayDecodeFailure.None, null);
        }

        public static ReplayDecodeResult Failed(ReplayDecodeFailure failure, string message)
        {
            return new ReplayDecodeResult(null, failure, message);
        }
    }
}
