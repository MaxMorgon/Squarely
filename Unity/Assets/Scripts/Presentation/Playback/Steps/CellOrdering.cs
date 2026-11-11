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
        /// <param name="placedCells">The cells of the placed piece, for <see cref="CellOrder.FromPlacement"/>.</param>
        public static float[] Delays(CellOrder order, IReadOnlyList<GridPos> cells, IReadOnlyList<GridPos> placedCells, float stagger)
        {
            var delays = new float[cells.Count];

            if (cells.Count == 0)
                return delays;

            if (order == CellOrder.FromPlacement && (placedCells == null || placedCells.Count == 0))
                order = CellOrder.Listed;

            for (int i = 0; i < cells.Count; i++)
                delays[i] = Key(order, i, cells, placedCells);

            float min = Mathf.Min(delays);

            for (int i = 0; i < delays.Length; i++)
                delays[i] = (delays[i] - min) * stagger;

            return delays;
        }

        private static float Key(CellOrder order, int index, IReadOnlyList<GridPos> cells, IReadOnlyList<GridPos> placedCells)
        {
            switch (order)
            {
                case CellOrder.Listed:
                    return index;

                case CellOrder.FromPlacement:
                    return DistanceToNearest(cells[index], placedCells);

                case CellOrder.FromCenter:
                    return Vector2.Distance(ToVector(cells[index]), Center(cells));

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
