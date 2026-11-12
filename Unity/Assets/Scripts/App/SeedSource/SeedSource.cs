using System;

namespace Squarely.App
{
    /// <summary>
    /// Where every new game seed comes from. Seeds are short (below <see cref="SeedLimit"/>), so players can
    /// read them out and type them.
    /// </summary>
    public sealed class SeedSource
    {
        /// <summary>New seeds are below this.</summary>
        public const int SeedLimit = 1000000000;

        private readonly Random _random;

        /// <param name="random">Picks the seeds. Give a seeded one for a known sequence.</param>
        public SeedSource(Random random)
        {
            _random = random ?? throw new ArgumentNullException(nameof(random));
        }

        /// <summary>A new seed, from 0 up to <see cref="SeedLimit"/> (not included).</summary>
        public ulong Next()
        {
            return (ulong)_random.Next(SeedLimit);
        }
    }
}
