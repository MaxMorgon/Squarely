namespace Squarely.Presentation
{
    /// <summary>
    /// Decides when a count-up plays a tick: when the shown number changes, but not sooner than a minimum time after the last tick.
    /// </summary>
    public struct CountTickGate
    {
        private readonly float _minSeconds;
        private int _lastValue;
        private float _lastTickSeconds;

        public CountTickGate(int startValue, float minSeconds)
        {
            _minSeconds = minSeconds;
            _lastValue = startValue;
            _lastTickSeconds = float.NegativeInfinity;
        }

        /// <summary>Reports the number shown at a time. Returns true if a tick plays now.</summary>
        public bool Advance(int value, float seconds)
        {
            if (value == _lastValue)
                return false;

            _lastValue = value;

            if (seconds - _lastTickSeconds < _minSeconds)
                return false;

            _lastTickSeconds = seconds;
            return true;
        }
    }
}
