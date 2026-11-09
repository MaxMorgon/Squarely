namespace Squarely.Presentation
{
    /// <summary>
    /// What a cell view is showing. The prefab decides how each state looks.
    /// </summary>
    public enum CellViewState
    {
        /// <summary>A placed cell, or a piece in the hand or being dragged.</summary>
        Idle,

        /// <summary>The landing preview of a dragged piece on the board.</summary>
        Ghost,
    }
}
