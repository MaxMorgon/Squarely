using System;
using NUnit.Framework;

namespace Squarely.App.Tests
{
    public sealed class SeedSourceTests
    {
        [Test]
        public void Next_AlwaysBelowLimit()
        {
            var source = new SeedSource(new Random(1));

            for (int i = 0; i < 1000; i++)
                Assert.That(source.Next(), Is.LessThan((ulong)SeedSource.SeedLimit));
        }

        [Test]
        public void Next_SameRandomSeed_SameSequence()
        {
            var first = new SeedSource(new Random(7));
            var second = new SeedSource(new Random(7));

            for (int i = 0; i < 10; i++)
                Assert.That(first.Next(), Is.EqualTo(second.Next()));
        }

        [Test]
        public void Next_Twice_GivesDifferentSeeds()
        {
            var source = new SeedSource(new Random(1));

            Assert.That(source.Next(), Is.Not.EqualTo(source.Next()));
        }

        [Test]
        public void Constructor_NullRandom_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new SeedSource(null));
        }
    }
}
