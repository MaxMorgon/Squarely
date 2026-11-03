using System.Collections.Generic;
using NUnit.Framework;

namespace Squarely.Simulation.Tests
{
    public class ScoringTests
    {
        private DefaultScoringRule _rule;

        [SetUp]
        public void SetUp()
        {
            // Defaults: 1 per cell, +10 per extra match, +50% per combo step.
            _rule = new DefaultScoringRule(new ScoringConfig());
        }

        [Test]
        public void NoMatch_ScoresCellsOnly_AndResetsCombo()
        {
            ScoreResult result = _rule.Score(4, Matches(), 3);

            Assert.That(result.Points, Is.EqualTo(4));
            Assert.That(result.NewCombo, Is.EqualTo(0));
        }

        [Test]
        public void SingleMatch_ScoresPatternPoints()
        {
            ScoreResult result = _rule.Score(3, Matches(10), 0);

            Assert.That(result.Points, Is.EqualTo(3 + 10));
            Assert.That(result.NewCombo, Is.EqualTo(1));
        }

        [Test]
        public void PatternsWithDifferentPoints_AreSummed_WithMultiMatchBonus()
        {
            ScoreResult result = _rule.Score(5, Matches(10, 50), 0);

            Assert.That(result.Points, Is.EqualTo(5 + 60 + 10));
        }

        [Test]
        public void SecondConsecutiveMatch_GetsComboBonus()
        {
            ScoreResult result = _rule.Score(1, Matches(10), 1);

            Assert.That(result.Points, Is.EqualTo(1 + 15));
            Assert.That(result.NewCombo, Is.EqualTo(2));
        }

        private static List<PatternMatch> Matches(params int[] points)
        {
            var matches = new List<PatternMatch>();

            for (int i = 0; i < points.Length; i++)
            {
                BoardPattern pattern = BoardPattern.FromText($"p{i}", "#", points[i], null);
                matches.Add(new PatternMatch(pattern, new GridPos(i, 0), new[] { new GridPos(i, 0) }));
            }

            return matches;
        }
    }
}
