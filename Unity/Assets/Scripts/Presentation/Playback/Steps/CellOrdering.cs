using System;
using System.Collections.Generic;
using Squarely.Simulation;
using UnityEngine;

namespace Squarely.Presentation
{
    /// <summary>Start delays for a staggered animation over a group of cells.</summary>
    public static class CellOrdering
    {
        /// <summary>
        /// The start delay of each cell, in seconds, in the order of <paramref name="cells"/>. The first cell reached starts at 0.
        /// For distance orders each cell unit further adds <paramref name="stagger"/>, so cells at the same distance start together.
        /// </summary>
        /// <param name="placedCells">The cells of the placed piece, for <see cref="CellOrder.FromPlacement"/> and <see cref="CellOrder.FromPointer"/>.</param>
        /// <param name="dropPoint">Where the pointer dropped the piece, in board cell coordinates (<see cref="BoardSpace.CellCoordinates"/>), or null.</param>
        public static float[] Delays(CellOrder order, IReadOnlyList<GridPos> cells, IReadOnlyList<GridPos> placedCells, Vector2? dropPoint,
            float stagger)
        {
            var delays = new float[cells.Count];

            if (cells.Count == 0)
                return delays;

            bool hasPlaced = placedCells != null && placedCells.Count > 0;

            if (order == CellOrder.FromPlacement && !hasPlaced)
                order = CellOrder.Listed;

            Vector2 origin = Origin(order, cells, placedCells, dropPoint);

            for (int i = 0; i < cells.Count; i++)
                delays[i] = Key(order, i, cells, placedCells, origin);

            float min = Mathf.Min(delays);

            for (int i = 0; i < delays.Length; i++)
                delays[i] = (delays[i] - min) * stagger;

            return delays;
        }

        /// <summary>The point distance orders measure from.</summary>
        private static Vector2 Origin(CellOrder order, IReadOnlyList<GridPos> cells, IReadOnlyList<GridPos> placedCells, Vector2? dropPoint)
        {
            if (order == CellOrder.FromPointer)
            {
                if (dropPoint.HasValue)
                    return dropPoint.Value;

                if (placedCells != null && placedCells.Count > 0)
                    return Center(placedCells);
            }

            return Center(cells);
        }

        private static float Key(CellOrder order, int index, IReadOnlyList<GridPos> cells, IReadOnlyList<GridPos> placedCells, Vector2 origin)
        {
            switch (order)
            {
                case CellOrder.Listed:
                    return index;

                case CellOrder.FromPlacement:
                    return DistanceToNearest(cells[index], placedCells);

                case CellOrder.FromCenter:
                case CellOrder.FromPointer:
                    return Vector2.Distance(ToVector(cells[index]), origin);

                default:
                    throw new ArgumentOutOfRangeException(nameof(order), order, null);
            }
        }

        private static float DistanceToNearest(GridPos cell, IReadOnlyList<GridPos> others)
        {
            float nearest = float.MaxValue;

            foreach (GridPos other in others)
                nearest = Mathf.Min(nearest, Vector2.Distance(ToVector(cell), ToVector(other)));

            return nearest;
        }

        private static Vector2 Center(IReadOnlyList<GridPos> cells)
        {
            Vector2 sum = Vector2.zero;

            foreach (GridPos cell in cells)
                sum += ToVector(cell);

            return sum / cells.Count;
        }

        private static Vector2 ToVector(GridPos pos)
        {
            return new Vector2(pos.X, pos.Y);
        }
    }
}
