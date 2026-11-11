using System.Collections.Generic;
using NUnit.Framework;

namespace Squarely.Simulation.Tests
{
    public class WeightedRandomDealerTests
    {
        private const int Seeds = 200;

        // Only the dot can complete a line (the bottom row); the square fits but completes nothing.
        private const string OnlyDotCompletes = "..../..../..../###.";

        // Checkerboard: every row and column has 2 of 4 cells filled, so nothing can complete a line.
        // Every 2x2 window has a filled cell, so the dot fits and the square doesn't.
        private const string OnlyDotFits = "#.#./.#.#/#.#./.#.#";

        private const string Full = "####/####/####/####";

        #region Guarantee on

        [Test]
        public void GuaranteeOn_WhenSomePieceCompletesAPattern_EveryHandHasOne()
        {
            Sim sim = CreateBoard(OnlyDotCompletes);
            WeightedRandomDealer dealer = CreateDealer(guarantee: true);

            for (ulong seed = 1; seed <= Seeds; seed++)
                Assert.That(CountDots(dealer.DealHand(sim, new DeterministicRandom(seed))), Is.GreaterThan(0), $"seed {seed}");
        }

        [Test]
        public void GuaranteeOn_WhenNothingCompletes_EveryHandHasAPieceThatFits()
        {
            Sim sim = CreateBoard(OnlyDotFits);
            WeightedRandomDealer dealer = CreateDealer(guarantee: true);

            for (ulong seed = 1; seed <= Seeds; seed++)
                Assert.That(CountDots(dealer.DealHand(sim, new DeterministicRandom(seed))), Is.GreaterThan(0), $"seed {seed}");
        }

        [Test]
        public void GuaranteeOn_RedealsOneSlotOnly()
        {
            Sim sim = CreateBoard(OnlyDotCompletes);
            WeightedRandomDealer dealer = CreateDealer(guarantee: true);
            WeightedRandomDealer plain = CreateDealer(guarantee: false);

            for (ulong seed = 1; seed <= Seeds; seed++)
            {
                IReadOnlyList<PieceShape> before = plain.DealHand(sim, new DeterministicRandom(seed));

                if (CountDots(before) > 0)
                    continue;

                IReadOnlyList<PieceShape> after = dealer.DealHand(sim, new DeterministicRandom(seed));

                Assert.That(CountDots(after), Is.EqualTo(1), $"seed {seed}");
                Assert.That(CountDifferences(before, after), Is.EqualTo(1), $"seed {seed}");
            }
        }

        [Test]
        public void GuaranteeOn_WhenHandIsAlreadyHelpful_DealsAndDrawsExactlyLikeOff()
        {
            Sim sim = CreateBoard(OnlyDotCompletes);
            WeightedRandomDealer dealer = CreateDealer(guarantee: true);
            WeightedRandomDealer plain = CreateDealer(guarantee: false);
            int helpfulHands = 0;

            for (ulong seed = 1; seed <= Seeds; seed++)
            {
                var plainRandom = new DeterministicRandom(seed);
                IReadOnlyList<PieceShape> expected = plain.DealHand(sim, plainRandom);

                if (CountDots(expected) == 0)
                    continue;

                helpfulHands++;
                var random = new DeterministicRandom(seed);

                Assert.That(Ids(dealer.DealHand(sim, random)), Is.EqualTo(Ids(expected)), $"seed {seed}");
                Assert.That(random.State, Is.EqualTo(plainRandom.State), $"seed {seed}");
            }

            Assert.That(helpfulHands, Is.GreaterThan(0));
        }

        [Test]
        public void GuaranteeOn_WhenNothingFits_DealsLikeOff()
        {
            Sim sim = CreateBoard(Full);
            WeightedRandomDealer dealer = CreateDealer(guarantee: true);
            WeightedRandomDealer plain = CreateDealer(guarantee: false);

            for (ulong seed = 1; seed <= Seeds; seed++)
            {
                IReadOnlyList<PieceShape> expected = plain.DealHand(sim, new DeterministicRandom(seed));

                Assert.That(Ids(dealer.DealHand(sim, new DeterministicRandom(seed))), Is.EqualTo(Ids(expected)), $"seed {seed}");
            }
        }

        #endregion

        #region Guarantee off

        [Test]
        public void GuaranteeOff_DealsHandsWithoutAHelpfulPiece()
        {
            Sim sim = CreateBoard(OnlyDotCompletes);
            WeightedRandomDealer dealer = CreateDealer(guarantee: false);
            int unhelpfulHands = 0;

            for (ulong seed = 1; seed <= Seeds; seed++)
            {
                if (CountDots(dealer.DealHand(sim, new DeterministicRandom(seed))) == 0)
                    unhelpfulHands++;
            }

            Assert.That(unhelpfulHands, Is.GreaterThan(0));
        }

        #endregion

        #region Helpers

        /// <summary>A 4x4 sim with a hand of 3, its board filled from the pattern. Its own hand is not used.</summary>
        private static Sim CreateBoard(string pattern)
        {
            var sim = new Sim(TestHelpers.SmallConfig(4, 4, 3), 1);
            TestHelpers.Fill(sim.EditableBoard, pattern);
            return sim;
        }

        /// <summary>A pool of a rare dot and a common 2x2 square.</summary>
        private static WeightedRandomDealer CreateDealer(bool guarantee)
        {
            var pieces = new List<PieceDefinition>
            {
                new PieceDefinition("dot", "#", weight: 1),
                new PieceDefinition("square2", "##/##", weight: 20),
            };

            return new WeightedRandomDealer(pieces, new DealingConfig { GuaranteeHelpfulPiece = guarantee });
        }

        private static int CountDots(IReadOnlyList<PieceShape> hand)
        {
            int count = 0;

            foreach (PieceShape shape in hand)
            {
                if (shape.Id == "dot")
                    count++;
            }

            return count;
        }

        private static int CountDifferences(IReadOnlyList<PieceShape> a, IReadOnlyList<PieceShape> b)
        {
            int count = 0;

            for (int i = 0; i < a.Count; i++)
            {
                if (a[i].Id != b[i].Id)
                    count++;
            }

            return count;
        }

        private static List<string> Ids(IReadOnlyList<PieceShape> hand)
        {
            var ids = new List<string>();

            foreach (PieceShape shape in hand)
                ids.Add(shape.Id);

            return ids;
        }

        #endregion
    }
}
