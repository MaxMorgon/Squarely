using System;
using UnityEngine;

namespace Squarely.Presentation
{
    /// <summary>
    /// The root of a cell visual prefab. Code sets its state; other components on the prefab decide how each state looks.
    /// Instances come from a <see cref="CellVisualCatalog"/> and go back to it with <see cref="Release"/>.
    /// </summary>
    public sealed class CellVisual : MonoBehaviour
    {
        private Action<CellVisual> _release;

        public CellVisualState State { get; private set; }

        public event Action<CellVisualState> StateChanged;

        public void SetState(CellVisualState state)
        {
            State = state;
            StateChanged?.Invoke(state);
        }

        /// <summary>Returns this visual to the pool it came from.</summary>
        public void Release()
        {
            if (_release == null)
                throw new InvalidOperationException($"{name} did not come from a {nameof(CellVisualCatalog)}.");

            _release(this);
        }

        internal void SetReleaseAction(Action<CellVisual> release)
        {
            _release = release;
        }
    }
}
