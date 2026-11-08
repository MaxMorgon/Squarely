using System;

namespace Squarely.Presentation
{
    /// <summary>
    /// One cell type and the visual prefab that shows it.
    /// </summary>
    [Serializable]
    public sealed class ThemeEntry
    {
        public string TypeId;

        public CellVisual Visual;
    }
}
