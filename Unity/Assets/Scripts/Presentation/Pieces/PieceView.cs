using System.Collections.Generic;
using Squarely.Simulation;
using UnityEngine;

namespace Squarely.Presentation
{
    /// <summary>
    /// Shows one piece shape, centered on this transform. Used for hand pieces, the dragged piece and the ghost.
    /// </summary>
    public sealed class PieceView : MonoBehaviour
    {
        private readonly List<CellView> _views = new List<CellView>();

        public PieceShape Shape { get; private set; }

        /// <summary>Local position of the center of the shape's origin cell (offset 0,0).</summary>
        public Vector2 OriginLocal { get; private set; }

        public void Show(PieceShape shape, CellViewPool viewPool, float cellSize, CellViewState state)
        {
            Clear();

            Shape = shape;
            OriginLocal = new Vector2(-(shape.Width - 1) * 0.5f * cellSize, -(shape.Height - 1) * 0.5f * cellSize);

            foreach (GridPos offset in shape.Offsets)
            {
                CellView view = viewPool.Get(shape.CellTypeId, transform);
                view.transform.localPosition = OriginLocal + new Vector2(offset.X, offset.Y) * cellSize;
                view.SetState(state);
                _views.Add(view);
            }
        }

        /// <summary>Releases all views, leaving the view empty.</summary>
        public void Clear()
        {
            foreach (CellView view in _views)
                view.Release();

            _views.Clear();
            Shape = null;
        }
    }
}
