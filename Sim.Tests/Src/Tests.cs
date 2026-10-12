using NUnit.Framework;

namespace Squarely.Sim.Tests
{
    public class Tests
    {
        [Test]
        public void GameState_CanBeCreated()
        {
            var state = new GameState();
            Assert.That(state, Is.Not.Null);
        }
    }
}