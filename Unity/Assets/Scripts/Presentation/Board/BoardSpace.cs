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

        public bool IsWithinBounds(GridPos pos)
        {
            return pos.X >= 0 && pos.X < Width && pos.Y >= 0 && pos.Y < Height;
        }

        /// <summary>Local position of a cell's center. Works for cells outside the board too.</summary>
        public Vector2 CellCenter(GridPos pos)
        {
            float x = (pos.X - (Width - 1) * 0.5f) * CellSize;
            float y = (pos.Y - (Height - 1) * 0.5f) * CellSize;
            return new Vector2(x, y);
        }

        /// <summary>The cell whose center is nearest to a local position. It may be outside the board.</summary>
        public GridPos NearestCell(Vector2 localPosition)
        {
            int x = Mathf.FloorToInt(localPosition.x / CellSize + (Width - 1) * 0.5f + 0.5f);
            int y = Mathf.FloorToInt(localPosition.y / CellSize + (Height - 1) * 0.5f + 0.5f);
            return new GridPos(x, y);
        }
    }
}
