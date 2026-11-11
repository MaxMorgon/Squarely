using System;
using System.Collections.Generic;
using UnityEngine;

namespace Squarely.Presentation
{
    /// <summary>Animates the target's cells: scale and tint over time, each cell starting after a stagger.</summary>
    [Serializable]
    public sealed class AnimateCells : CueStep
    {
        [Tooltip("Seconds each cell animates.")]
        [SerializeField] private float _duration = 0.25f;

        [Tooltip("Seconds between one cell and the next (one cell further away, for distance orders).")]
        [SerializeField] private float _stagger = 0.03f;

        [SerializeField] private CellOrder _order = CellOrder.Listed;

        [Tooltip("Cell scale over the animation (time 0 to 1).")]
        [SerializeField] private AnimationCurve _scale = AnimationCurve.Constant(0f, 1f, 1f);

        [Tooltip("Color multiplied over the cell over the animation. Alpha fades it.")]
        [SerializeField] private Gradient _tint = new Gradient();

        public override async Awaitable Play(CueTarget target, PlaybackContext context)
        {
            IReadOnlyList<CellView> cells = target.Cells;

            if (cells.Count == 0)
                return;

            float[] delays = CellOrdering.Delays(_order, target.CellPositions, context.PlacedCells, context.DropPoint, _stagger);
            float end = Mathf.Max(delays) + _duration;
            float elapsed = 0f;

            SetLooks(cells, delays, elapsed);

            while (elapsed < end)
            {
                if (!await context.NextFrameAsync(target))
                    return;

                elapsed += context.DeltaTime;
                SetLooks(cells, delays, elapsed);
            }
        }

        public override void Snap(CueTarget target, PlaybackContext context)
        {
            foreach (CellView cell in target.Cells)
                SetLook(cell, 1f);
        }

        private void SetLooks(IReadOnlyList<CellView> cells, float[] delays, float elapsed)
        {
            for (int i = 0; i < cells.Count; i++)
                SetLook(cells[i], Progress(elapsed - delays[i], _duration));
        }

        private void SetLook(CellView cell, float t)
        {
            cell.transform.localScale = Vector3.one * _scale.Evaluate(t);
            cell.SetTint(_tint.Evaluate(t));
        }
    }
}
