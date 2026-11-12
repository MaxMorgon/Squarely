using NUnit.Framework;
using Squarely.Simulation;

namespace Squarely.App.Tests
{
    public sealed class GameOverJingleTests
    {
        [Test]
        public void Choose_NewRecord_PlaysRecordJingleOnly()
        {
            Assert.That(GameOverJingle.Choose(GameOutcome.Lost, true), Is.EqualTo(GameOverJingle.Kind.NewRecord));
            Assert.That(GameOverJingle.Choose(GameOutcome.Won, true), Is.EqualTo(GameOverJingle.Kind.NewRecord));
        }

        [Test]
        public void Choose_NoRecord_PlaysOutcomeJingle()
        {
            Assert.That(GameOverJingle.Choose(GameOutcome.Lost, false), Is.EqualTo(GameOverJingle.Kind.Lost));
            Assert.That(GameOverJingle.Choose(GameOutcome.Won, false), Is.EqualTo(GameOverJingle.Kind.Won));
        }
    }
}
