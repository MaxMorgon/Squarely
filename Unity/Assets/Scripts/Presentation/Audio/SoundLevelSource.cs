namespace Squarely.Presentation
{
    /// <summary>Where a <see cref="PlaySound"/> step reads the level that raises the pitch.</summary>
    public enum SoundLevelSource
    {
        /// <summary>Level 0.</summary>
        None,

        /// <summary>The end value of the target's count.</summary>
        Count,

        /// <summary>The number of matches the command clears.</summary>
        ClearCount,

        /// <summary>The combo after the command.</summary>
        ComboLevel
    }
}
