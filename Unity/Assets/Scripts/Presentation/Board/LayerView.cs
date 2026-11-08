using Squarely.Simulation;
using UnityEngine;

namespace Squarely.Presentation
{
    /// <summary>
    /// One grid of cell visuals on the board, with a visual only where a cell is filled.
    /// </summary>
    public sealed class LayerView : MonoBehaviour
    {
        private BoardLayout _layout;
        private CellVisualCatalog _catalog;
        private SlotGrid<CellVisual> _slots;

        public void Initialize(BoardLayout layout, CellVisualCatalog catalog)
        {
            _layout = layout;
            _catalog = catalog;
            _slots = new SlotGrid<CellVisual>(layout.Width, layout.Height);
        }

        /// <summary>Shows a visual of the cell type in an empty slot.</summary>
        public CellVisual Spawn(GridPos pos, string typeId)
        {
            CellVisual visual = _catalog.Get(typeId, transform);
            visual.transform.localPosition = _layout.CellCenter(pos);
            _slots.Set(pos, visual);
            return visual;
        }

        /// <summary>
        /// Removes the visual from its slot and returns it, or null if the slot is empty.
        /// The caller then owns the visual and releases it.
        /// </summary>
        public CellVisual Detach(GridPos pos)
        {
            return _slots.Detach(pos);
        }

        /// <summary>Releases every visual and spawns one for each filled cell of the board.</summary>
        public void Rebuild(IReadOnlyBoard board)
        {
            for (int y = 0; y < _layout.Height; y++)
            {
                for (int x = 0; x < _layout.Width; x++)
                {
                    var pos = new GridPos(x, y);
                    CellVisual old = _slots.Detach(pos);

                    if (old != null)
                        old.Release();

                    Cell cell = board.GetCell(pos);

                    if (!cell.IsEmpty)
                        Spawn(pos, cell.TypeId);
                }
            }
        }
    }
}
