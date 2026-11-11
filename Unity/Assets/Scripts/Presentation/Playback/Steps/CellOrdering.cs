using System;
using System.Collections.Generic;
using Squarely.Simulation;
using UnityEngine;

namespace Squarely.Presentation
{
    /// <summary>Start delays for a staggered animation over a group of cells.</summary>
    public static class CellOrdering
    {
        /// <summary>Distances closer than this, in cell units, count as equal: those cells start together.</summary>
        private const float SameKeyTolerance = 0.001f;

        /// <summary>
        /// The start delay of each cell, in seconds, in the order of <paramref name="cells"/>. The first cell reached starts at 0,
        /// and each later step in the order starts <paramref name="stagger"/> after the one before, however far the cells are apart.
        /// Cells at the same distance share a step and start together.
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

            return ToSteps(delays, stagger);
        }

        /// <summary>Replaces each key with its step in the order (equal keys share a step) times <paramref name="stagger"/>.</summary>
        private static float[] ToSteps(float[] keys, float stagger)
        {
            var sorted = (float[])keys.Clone();
            Array.Sort(sorted);

            var delays = new float[keys.Length];

            for (int i = 0; i < keys.Length; i++)
                delays[i] = StepOf(keys[i], sorted) * stagger;

            return delays;
        }

        /// <summary>The number of distinct keys in <paramref name="sorted"/> that come before <paramref name="key"/>.</summary>
        private static int StepOf(float key, float[] sorted)
        {
            int step = 0;

            for (int i = 1; i < sorted.Length; i++)
            {
                if (sorted[i] - key > SameKeyTolerance)
                    break;

                if (sorted[i] - sorted[i - 1] > SameKeyTolerance)
                    step++;
            }

            return step;
        }

        /// <summary>The point distance orders measure from.</summary>
        private static Vector2 Origin(CellOrder order, IReadOnlyList<GridPos> cells, IReadOnlyList<GridPos> placedCells, Vector2? dropPoint)
        {
            if (order == CellOrder.FromPointer)
            {
                bool hasPlaced = placedCells != null && placedCells.Count > 0;

                if (dropPoint.HasValue)
                    return ToVector(NearestTo(dropPoint.Value, hasPlaced ? placedCells : cells));

                if (hasPlaced)
                    return Center(placedCells);
            }

            return Center(cells);
        }

        /// <summary>The first of <paramref name="cells"/> nearest to <paramref name="point"/>.</summary>
        private static GridPos NearestTo(Vector2 point, IReadOnlyList<GridPos> cells)
        {
            GridPos nearest = cells[0];
            float nearestDistance = float.MaxValue;

            foreach (GridPos cell in cells)
            {
                float distance = Vector2.Distance(ToVector(cell), point);

                if (distance < nearestDistance - SameKeyTolerance)
                {
                    nearest = cell;
                    nearestDistance = distance;
                }
            }

            return nearest;
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
