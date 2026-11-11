using System;
using System.Collections.Generic;
using Squarely.Simulation;
using UnityEngine;

namespace Squarely.Presentation
{
    /// <summary>
    /// One grid of cell views on the board, with a view only where a cell is filled.
    /// </summary>
    public sealed class LayerView : MonoBehaviour
    {
        private BoardSpace _boardSpace;
        private CellViewPool _viewPool;
        private Grid<CellView> _views;

        public void Initialize(BoardSpace boardSpace, CellViewPool viewPool)
        {
            _boardSpace = boardSpace;
            _viewPool = viewPool;
            _views = new Grid<CellView>(boardSpace.Width, boardSpace.Height);
        }

        /// <summary>The view in a slot, or null if the slot has none.</summary>
        public CellView GetView(GridPos pos)
        {
            return _views[pos];
        }

        /// <summary>Every slot that has a view, in board order (left to right, bottom to top).</summary>
        public List<GridPos> GetFilledSlots()
        {
            var slots = new List<GridPos>();

            for (int y = 0; y < _views.Height; y++)
            {
                for (int x = 0; x < _views.Width; x++)
                {
                    var pos = new GridPos(x, y);

                    if (_views[pos] != null)
                        slots.Add(pos);
                }
            }

            return slots;
        }

        /// <summary>World position of a cell's center. Works for cells outside the board too.</summary>
        public Vector3 CellWorldPosition(GridPos pos)
        {
            return transform.TransformPoint(_boardSpace.CellCenter(pos));
        }

        /// <summary>Shows a view of the cell type in an empty slot.</summary>
        public CellView Spawn(GridPos pos, string typeId)
        {
            if (_views[pos] != null)
                throw new InvalidOperationException($"Slot {pos} already has a view.");

            CellView view = _viewPool.Get(typeId, transform);
            view.transform.localPosition = _boardSpace.CellCenter(pos);
            _views[pos] = view;
            return view;
        }

        /// <summary>
        /// Removes the view from its slot and returns it, or null if the slot is empty.
        /// The caller then owns the view and releases it.
        /// </summary>
        public CellView Detach(GridPos pos)
        {
            CellView view = _views[pos];
            _views[pos] = null;
            return view;
        }

        /// <summary>Releases every view and spawns one for each filled cell of the board.</summary>
        public void Rebuild(IReadOnlyBoard board)
        {
            for (int y = 0; y < _boardSpace.Height; y++)
            {
                for (int x = 0; x < _boardSpace.Width; x++)
                {
                    var pos = new GridPos(x, y);
                    CellView old = Detach(pos);

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
