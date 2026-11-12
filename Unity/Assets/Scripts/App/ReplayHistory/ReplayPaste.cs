using Squarely.Simulation;

namespace Squarely.App
{
    /// <summary>Checks a pasted replay code against the current game mode.</summary>
    public static class ReplayPaste
    {
        public static ReplayPasteResult Check(string text, SimConfig config)
        {
            if (string.IsNullOrWhiteSpace(text))
                return new ReplayPasteResult(ReplayPasteStatus.Empty, null, null);

            ReplayDecodeResult decoded = ReplayCodec.Decode(text);

            if (!decoded.Success)
                return new ReplayPasteResult(ReplayPasteStatus.Invalid, null, decoded.Message);

            return Check(decoded.Replay, config);
        }

        /// <summary>Checks a replay that is already decoded, for its game config.</summary>
        public static ReplayPasteResult Check(GameReplay replay, SimConfig config)
        {
            bool matches = replay.ConfigFingerprint == ConfigFingerprint.Compute(config);
            return new ReplayPasteResult(matches ? ReplayPasteStatus.Ok : ReplayPasteStatus.ConfigMismatch, replay, null);
        }
    }
}
