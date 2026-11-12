namespace Squarely.App
{
    /// <summary>What <see cref="ReplayPaste.Check"/> found in a pasted text.</summary>
    public enum ReplayPasteStatus
    {
        /// <summary>The text is empty or only spaces.</summary>
        Empty,

        /// <summary>The text is not a replay code.</summary>
        Invalid,

        /// <summary>A replay code, recorded with a different game config than the current one.</summary>
        ConfigMismatch,

        /// <summary>A replay code, recorded with the current game config.</summary>
        Ok,
    }
}
