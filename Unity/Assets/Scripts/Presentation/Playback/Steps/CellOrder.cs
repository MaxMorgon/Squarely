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

        /// <summary>
        /// Outward from the placed piece's cell nearest to where the pointer dropped it (the target's nearest cell if nothing
        /// was placed). Without a drop point (e.g. a scripted placement), from the placed piece's center, or from the target's
        /// center if nothing was placed.
        /// </summary>
        FromPointer,
    }
}
