using System;
using UnityEngine;

namespace Squarely.Presentation
{
    /// <summary>
    /// The root of a cell view prefab. Code sets its state; other components on the prefab decide how each state looks.
    /// Instances come from a <see cref="CellViewPool"/> and go back to it with <see cref="Release"/>.
    /// </summary>
    public sealed class CellView : MonoBehaviour
    {
        private Action<CellView> _release;

        public CellViewState State { get; private set; }

        public event Action<CellViewState> StateChanged;

        public void SetState(CellViewState state)
        {
            State = state;
            StateChanged?.Invoke(state);
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
    }
}
