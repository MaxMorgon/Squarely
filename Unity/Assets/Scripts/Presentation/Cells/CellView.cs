using System;
using UnityEngine;

namespace Squarely.Presentation
{
    /// <summary>
    /// The root of a cell view prefab. Code sets its state and tint; other components on the prefab decide how they look.
    /// Instances come from a <see cref="CellViewPool"/> and go back to it with <see cref="Release"/>.
    /// </summary>
    public sealed class CellView : MonoBehaviour
    {
        private Action<CellView> _release;

        /// <summary>The cell type this view shows.</summary>
        public string TypeId { get; private set; }

        public CellViewState State { get; private set; }

        /// <summary>A color multiplied over the view's look, white by default. Cues use it to tint and fade cells.</summary>
        public Color Tint { get; private set; } = Color.white;

        /// <summary>Which look variant the view shows, 0 by default. The prefab maps it to a look (e.g. <see cref="CellVariantSprite"/>).</summary>
        public int Variant { get; private set; }

        public event Action<CellViewState> StateChangeEvent;

        public event Action<Color> TintChangeEvent;

        public event Action<int> VariantChangeEvent;

        public void SetState(CellViewState state)
        {
            State = state;
            StateChangeEvent?.Invoke(state);
        }

        public void SetTint(Color tint)
        {
            Tint = tint;
            TintChangeEvent?.Invoke(tint);
        }

        public void SetVariant(int variant)
        {
            Variant = variant;
            VariantChangeEvent?.Invoke(variant);
        }

        /// <summary>Returns this view to the pool it came from.</summary>
        public void Release()
        {
            if (_release == null)
                throw new InvalidOperationException($"{name} did not come from a {nameof(CellViewPool)}.");

            _release(this);
        }

        internal void SetReleaseAction(Action<CellView> release)
        {
            _release = release;
        }

        internal void SetTypeId(string typeId)
        {
            TypeId = typeId;
        }
    }
}
