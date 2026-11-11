using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Squarely.Simulation.Tests
{
    public class WeightedRandomDealerTests
    {
        [Test]
        public void Deal_SkipsZeroWeightPieces()
        {
            var dealer = new WeightedRandomDealer(new[]
            {
                new PieceDefinition("never", "#", 0),
                new PieceDefinition("always", "##", 1),
            });
            var random = new DeterministicRandom(1);

            for (int i = 0; i < 200; i++)
                Assert.That(dealer.Deal(null, random).Id, Is.EqualTo("always"));
        }

        [Test]
        public void Deal_FollowsWeights()
        {
            var dealer = new WeightedRandomDealer(new[]
            {
                new PieceDefinition("light", "#", 1),
                new PieceDefinition("heavy", "##", 3),
            });
            var random = new DeterministicRandom(1);
            var counts = new Dictionary<string, int> { { "light", 0 }, { "heavy", 0 } };

            for (int i = 0; i < 4000; i++)
                counts[dealer.Deal(null, random).Id]++;

            // Expected 1000 / 3000; the seed is fixed, so the margin only has to fit this one sequence.
            Assert.That(counts["light"], Is.InRange(900, 1100));
            Assert.That(counts["heavy"], Is.InRange(2900, 3100));
        }

        [Test]
        public void Deal_ShapeHasDefinitionsPatternAndCellType()
        {
            var definition = new PieceDefinition("stone_l", "#./##") { CellTypeId = "stone" };
            var dealer = new WeightedRandomDealer(new[] { definition });

            PieceShape shape = dealer.Deal(null, new DeterministicRandom(1));

            Assert.That(shape.Id, Is.EqualTo("stone_l"));
            Assert.That(shape.CellCount, Is.EqualTo(3));
            Assert.That(shape.CellTypeId, Is.EqualTo("stone"));
        }

        [Test]
        public void Constructor_AllZeroWeights_Throws()
        {
            var pieces = new[] { new PieceDefinition("a", "#", 0), new PieceDefinition("b", "##", 0) };

            Assert.Throws<ArgumentException>(() => new WeightedRandomDealer(pieces));
        }

        [Test]
        public void Constructor_NullPieces_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new WeightedRandomDealer(null));
        }
    }
}
