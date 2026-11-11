namespace Squarely.Presentation
{
    /// <summary>The order in which a staggered animation reaches a target's cells.</summary>
    public enum CellOrder
    {
        /// <summary>In the order the target lists them (board order for clears: left to right, bottom to top).</summary>
        Listed,

        /// <summary>From the cells nearest the placed piece outward. Falls back to Listed if nothing was placed.</summary>
        FromPlacement,

        /// <summary>From the target's center outward, both ends of a line at once.</summary>
        FromCenter,
    }
}
