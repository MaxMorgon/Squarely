using UnityEngine;

namespace Squarely.Presentation
{
    /// <summary>A volume that moves in a straight line to a target.</summary>
    public sealed class VolumeFade
    {
        private float _rate;

        public float Current { get; private set; }

        public float Target { get; private set; }

        public VolumeFade(float start)
        {
            Current = start;
            Target = start;
        }

        /// <summary>Moves to a volume over a time. With 0 seconds or less, it jumps at once.</summary>
        public void FadeTo(float target, float seconds)
        {
            Target = target;

            if (seconds <= 0f)
            {
                Current = target;
                _rate = 0f;
                return;
            }

            _rate = Mathf.Abs(target - Current) / seconds;
        }

        public void Tick(float deltaSeconds)
        {
            Current = Mathf.MoveTowards(Current, Target, _rate * deltaSeconds);
        }
    }
}
