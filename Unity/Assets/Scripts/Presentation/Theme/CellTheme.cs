using System.Collections.Generic;
using UnityEngine;

namespace Squarely.Presentation
{
    /// <summary>
    /// How cells look: which view prefab shows each cell type, and the empty-slot tile.
    /// Swapping it reskins the board and pieces.
    /// </summary>
    [CreateAssetMenu(menuName = "Squarely/Cell Theme", fileName = "CellTheme")]
    public sealed class CellTheme : ScriptableObject
    {
        [SerializeField] private List<CellThemeEntry> _cellViews = new List<CellThemeEntry>();

        [Tooltip("Shown for cell types without an entry.")]
        [SerializeField] private CellView _fallbackCellView;

        [Tooltip("Shown in every board slot, behind the cells.")]
        [SerializeField] private GameObject _emptyTile;

        public GameObject EmptyTile => _emptyTile;

        /// <summary>The view prefab for a cell type, or the fallback if the type has no entry.</summary>
        public CellView ResolveCellView(string typeId)
        {
            foreach (CellThemeEntry entry in _cellViews)
            {
                if (entry.TypeId == typeId)
                    return entry.View;
            }

            return _fallbackCellView;
        }
    }
}
