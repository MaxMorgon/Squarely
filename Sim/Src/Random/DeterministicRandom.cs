using System;

namespace Squarely.Simulation
{
    /// <summary>
    /// Seeded xorshift64* PRNG. Produces identical sequences on every runtime (.NET, Mono, IL2CPP),
    /// unlike System.Random. The full state is a single ulong, so it is trivial to snapshot.
    /// </summary>
    public sealed class DeterministicRandom
    {
        private ulong _state;

        public DeterministicRandom(ulong seed)
        {
            _state = SplitMix64(seed);

            // xorshift must never have an all-zero state.
            if (_state == 0)
                _state = 0x9E3779B97F4A7C15UL;
        }

        public ulong State => _state;

        public static DeterministicRandom FromState(ulong state)
        {
            if (state == 0)
                throw new ArgumentException("State must be non-zero.", nameof(state));

            var random = new DeterministicRandom(0);
            random._state = state;
            return random;
        }

        /// <summary>An independent copy that continues with the same sequence.</summary>
        internal DeterministicRandom Clone()
        {
            return FromState(_state);
        }

        public ulong NextULong()
        {
            _state ^= _state >> 12;
            _state ^= _state << 25;
            _state ^= _state >> 27;
            return _state * 0x2545F4914F6CDD1DUL;
        }

        /// <summary>
        /// Returns a uniformly distributed value in [0, maxExclusive).
        /// </summary>
        public int NextInt(int maxExclusive)
        {
            if (maxExclusive <= 0)
                throw new ArgumentOutOfRangeException(nameof(maxExclusive), "Max must be positive.");

            ulong range = (ulong)maxExclusive;

            // Reject values from the incomplete final bucket so every result is equally likely.
            ulong limit = ulong.MaxValue - (ulong.MaxValue % range);
            ulong value;

            do
            {
                value = NextULong();
            }
            while (value >= limit);

            return (int)(value % range);
        }

        private static ulong SplitMix64(ulong value)
        {
            value += 0x9E3779B97F4A7C15UL;
            value = (value ^ (value >> 30)) * 0xBF58476D1CE4E5B9UL;
            value = (value ^ (value >> 27)) * 0x94D049BB133111EBUL;
            return value ^ (value >> 31);
        }
    }
}
