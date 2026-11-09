using System;

namespace Squarely.Presentation
{
    /// <summary>
    /// One cell type and the view prefab that shows it.
    /// </summary>
    [Serializable]
    public sealed class CellThemeEntry
    {
        public string TypeId;

        public CellView View;
    }
}
