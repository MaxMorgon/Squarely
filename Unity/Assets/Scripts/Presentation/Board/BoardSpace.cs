using Squarely.Simulation;
using UnityEngine;

namespace Squarely.Presentation
{
    /// <summary>
    /// Converts between board cells and positions in the board's local space. The board is centered on the local origin.
    /// </summary>
    public sealed class BoardSpace
    {
        public int Width { get; }

        public int Height { get; }

        public float CellSize { get; }

        public BoardSpace(int width, int height, float cellSize)
        {
            Width = width;
            Height = height;
            CellSize = cellSize;
        }

        /// <summary>The board's area in its local space.</summary>
        public Rect LocalBounds
        {
            get
            {
                float width = Width * CellSize;
                float height = Height * CellSize;
                return new Rect(-width * 0.5f, -height * 0.5f, width, height);
            }
        }

        /// <summary>Local position of a cell's center. Works for cells outside the board too.</summary>
        public Vector2 CellCenter(GridPos pos)
        {
            float x = (pos.X - (Width - 1) * 0.5f) * CellSize;
            float y = (pos.Y - (Height - 1) * 0.5f) * CellSize;
            return new Vector2(x, y);
        }

        /// <summary>
        /// A local position in cell units: a cell's center is at its (X, Y), positions between centers are fractional.
        /// Works for positions outside the board too.
        /// </summary>
        public Vector2 CellCoordinates(Vector2 localPosition)
        {
            float x = localPosition.x / CellSize + (Width - 1) * 0.5f;
            float y = localPosition.y / CellSize + (Height - 1) * 0.5f;
            return new Vector2(x, y);
        }

        /// <summary>The cell whose center is nearest to a local position. It may be outside the board.</summary>
        public GridPos NearestCell(Vector2 localPosition)
        {
            Vector2 coordinates = CellCoordinates(localPosition);
            return new GridPos(Mathf.FloorToInt(coordinates.x + 0.5f), Mathf.FloorToInt(coordinates.y + 0.5f));
        }
    }
}
