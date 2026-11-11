using System;
using System.Collections.Generic;
using Squarely.Simulation;
using UnityEngine;

namespace Squarely.Presentation
{
    /// <summary>
    /// What a cue sequence plays on, built by a handler: cell views, a transform, a number to count, and a position for effects.
    /// Steps use the parts they need and skip the ones a target doesn't have.
    /// </summary>
    public sealed class CueTarget
    {
        private readonly Action<int> _setCount;

        /// <summary>The cell views to animate. Never null.</summary>
        public IReadOnlyList<CellView> Cells { get; }

        /// <summary>The grid position of each cell in <see cref="Cells"/>, in the same order. Used to order staggers.</summary>
        public IReadOnlyList<GridPos> CellPositions { get; }

        /// <summary>A single transform to animate, e.g. a HUD label, or null.</summary>
        public Transform Transform { get; }

        /// <summary>World position for effects, e.g. a match's center.</summary>
        public Vector3 Position { get; }

        public int CountFrom { get; }

        public int CountTo { get; }

        public bool HasCount => _setCount != null;

        private CueTarget(IReadOnlyList<CellView> cells, IReadOnlyList<GridPos> cellPositions, Transform transform, Vector3 position,
            int countFrom, int countTo, Action<int> setCount)
        {
            Cells = cells ?? Array.Empty<CellView>();
            CellPositions = cellPositions ?? Array.Empty<GridPos>();
            Transform = transform;
            Position = position;
            CountFrom = countFrom;
            CountTo = countTo;
            _setCount = setCount;
        }

        /// <param name="positions">The grid position of each cell, in the same order as <paramref name="cells"/>.</param>
        public static CueTarget ForCells(IReadOnlyList<CellView> cells, IReadOnlyList<GridPos> positions, Vector3 position)
        {
            if (cells.Count != positions.Count)
                throw new ArgumentException("Every cell needs a position.", nameof(positions));

            return new CueTarget(cells, positions, null, position, 0, 0, null);
        }

        public static CueTarget ForTransform(Transform transform, Vector3 position)
        {
            return new CueTarget(null, null, transform, position, 0, 0, null);
        }

        /// <param name="setCount">Shows a value of the count.</param>
        /// <param name="transform">The transform showing the count, e.g. its label.</param>
        public static CueTarget ForCount(int from, int to, Action<int> setCount, Transform transform, Vector3 position)
        {
            return new CueTarget(null, null, transform, position, from, to, setCount);
        }

        /// <summary>Shows a value of the count. Does nothing if the target has no count.</summary>
        public void SetCount(int value)
        {
            _setCount?.Invoke(value);
        }
    }
}
