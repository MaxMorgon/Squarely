using NUnit.Framework;

namespace Squarely.Simulation.Tests
{
    public class HandTests
    {
        private static readonly PieceShape Single = PieceShape.FromPattern("single", "#");

        [Test]
        public void IsEmpty_NewHand_IsTrue()
        {
            var hand = new Hand(3);

            Assert.That(hand.IsEmpty, Is.True);
        }

        [Test]
        public void IsEmpty_WithOnePiece_IsFalse()
        {
            var hand = new Hand(3);
            hand.Add(new PieceInstance(1, Single));

            Assert.That(hand.IsEmpty, Is.False);
        }

        [Test]
        public void IsEmpty_AfterLastPieceTaken_IsTrue()
        {
            var hand = new Hand(2);
            hand.Add(new PieceInstance(1, Single));
            hand.Add(new PieceInstance(2, Single));

            hand.Take(1);
            Assert.That(hand.IsEmpty, Is.False);

            hand.Take(2);
            Assert.That(hand.IsEmpty, Is.True);
        }
    }
}
