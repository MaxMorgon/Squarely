using NUnit.Framework;

namespace Squarely.Simulation.Tests
{
    public class ScoringTests
    {
        private DefaultScoringRule _rule;

        [SetUp]
        public void SetUp()
        {
            // Defaults: 1 per cell, 10 per line, +10 per extra line, +50% per combo step.
            _rule = new DefaultScoringRule(new ScoringConfig());
        }

        [Test]
        public void NoClear_ScoresCellsOnly_AndResetsCombo()
        {
            ScoreResult result = _rule.Score(4, 0, 3);

            Assert.That(result.Points, Is.EqualTo(4));
            Assert.That(result.NewCombo, Is.EqualTo(0));
        }

        [Test]
        public void SingleLine_FirstCombo()
        {
            ScoreResult result = _rule.Score(3, 1, 0);

            Assert.That(result.Points, Is.EqualTo(3 + 10));
            Assert.That(result.NewCombo, Is.EqualTo(1));
        }

        [Test]
        public void TwoLines_GetMultiLineBonus()
        {
            ScoreResult result = _rule.Score(5, 2, 0);

            Assert.That(result.Points, Is.EqualTo(5 + 20 + 10));
        }

        [Test]
        public void SecondConsecutiveClear_GetsComboBonus()
        {
            ScoreResult result = _rule.Score(1, 1, 1);

            Assert.That(result.Points, Is.EqualTo(1 + 15));
            Assert.That(result.NewCombo, Is.EqualTo(2));
        }
    }
}
