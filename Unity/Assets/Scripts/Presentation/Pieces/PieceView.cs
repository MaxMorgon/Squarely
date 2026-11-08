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
        private readonly List<CellVisual> _visuals = new List<CellVisual>();

        public PieceShape Shape { get; private set; }

        /// <summary>Local position of the center of the shape's origin cell (offset 0,0).</summary>
        public Vector2 OriginLocal { get; private set; }

        /// <summary>World position of the center of the shape's origin cell.</summary>
        public Vector3 OriginWorld => transform.TransformPoint(OriginLocal);

        public void Show(PieceShape shape, CellVisualCatalog catalog, float cellSize, CellVisualState state)
        {
            Clear();

            Shape = shape;
            OriginLocal = new Vector2(-(shape.Width - 1) * 0.5f * cellSize, -(shape.Height - 1) * 0.5f * cellSize);

            foreach (GridPos offset in shape.Offsets)
            {
                CellVisual visual = catalog.Get(shape.CellTypeId, transform);
                visual.transform.localPosition = OriginLocal + new Vector2(offset.X, offset.Y) * cellSize;
                visual.SetState(state);
                _visuals.Add(visual);
            }
        }

        /// <summary>Releases all visuals, leaving the view empty.</summary>
        public void Clear()
        {
            foreach (CellVisual visual in _visuals)
                visual.Release();

            _visuals.Clear();
            Shape = null;
        }
    }
}
