namespace Squarely.Presentation
{
    /// <summary>
    /// What a cell visual is showing. The prefab decides how each state looks.
    /// </summary>
    public enum CellVisualState
    {
        /// <summary>A placed cell, or a piece in the hand or being dragged.</summary>
        Idle,

        /// <summary>The landing preview of a dragged piece on the board.</summary>
        Ghost,
    }
}
