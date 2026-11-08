using System.Collections.Generic;
using UnityEngine;

namespace Squarely.Presentation
{
    /// <summary>
    /// Decides how the game looks: which visual prefab shows each cell type, and the empty-slot tile.
    /// Swapping the theme reskins the game.
    /// </summary>
    [CreateAssetMenu(menuName = "Squarely/Theme", fileName = "Theme")]
    public sealed class Theme : ScriptableObject
    {
        [SerializeField] private List<ThemeEntry> _cellVisuals = new List<ThemeEntry>();

        [Tooltip("Shown for cell types without an entry.")]
        [SerializeField] private CellVisual _fallbackCellVisual;

        [Tooltip("Shown in every board slot, behind the cells.")]
        [SerializeField] private GameObject _emptyTile;

        public GameObject EmptyTile => _emptyTile;

        /// <summary>The visual prefab for a cell type, or the fallback if the type has no entry.</summary>
        public CellVisual ResolveCellVisual(string typeId)
        {
            foreach (ThemeEntry entry in _cellVisuals)
            {
                if (entry.TypeId == typeId)
                    return entry.Visual;
            }

            return _fallbackCellVisual;
        }
    }
}
