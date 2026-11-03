using System.Collections.Generic;
using NUnit.Framework;

namespace Squarely.Simulation.Tests
{
    public class SimTests
    {
        #region Setup

        [Test]
        public void NewSim_HasFullHand_AndEmptyBoard()
        {
            var sim = new Sim(DefaultSimConfig.Create(), 1);

            Assert.That(sim.Hand.Size, Is.EqualTo(3));

            for (int slot = 0; slot < sim.Hand.Size; slot++)
                Assert.That(sim.Hand.IsEmpty(slot), Is.False);

            Assert.That(sim.Score, Is.EqualTo(0));
            Assert.That(sim.Combo, Is.EqualTo(0));
            Assert.That(sim.IsGameOver, Is.False);
        }

        #endregion

        #region Placement

        [Test]
        public void Place_ValidMove_Succeeds_AndRefillsSlot()
        {
            var dealer = new ScriptedDealer("#", "##", "###", "#/#");
            var sim = new Sim(TestHelpers.SmallConfig(8, 8, 3), 1, dealer, null);

            CommandResult result = sim.Execute(new PlacePieceCommand(0, new GridPos(0, 0)));

            Assert.That(result.Success, Is.True);
            Assert.That(sim.Board.IsEmpty(new GridPos(0, 0)), Is.False);
            Assert.That(sim.Hand.Get(0).Id, Is.EqualTo("scripted_3"));

            PieceDealtEvent dealt = TestHelpers.Single<PieceDealtEvent>(result.Events);
            Assert.That(dealt.Slot, Is.EqualTo(0));
        }

        [Test]
        public void Place_OutOfBounds_FailsWithoutChangingState()
        {
            var dealer = new ScriptedDealer("###", "#", "#");
            var sim = new Sim(TestHelpers.SmallConfig(8, 8, 3), 1, dealer, null);

            CommandResult result = sim.Execute(new PlacePieceCommand(0, new GridPos(7, 0)));

            Assert.That(result.Success, Is.False);
            Assert.That(result.Events, Is.Empty);
            Assert.That(sim.Hand.Get(0).Id, Is.EqualTo("scripted_0"));
            Assert.That(sim.Board.IsEmpty(new GridPos(7, 0)), Is.True);
        }

        [Test]
        public void Place_InvalidSlot_Fails()
        {
            var sim = new Sim(TestHelpers.SmallConfig(8, 8, 3), 1);

            Assert.That(sim.Execute(new PlacePieceCommand(5, new GridPos(0, 0))).Success, Is.False);
            Assert.That(sim.Execute(new PlacePieceCommand(-1, new GridPos(0, 0))).Success, Is.False);
        }

        [Test]
        public void Place_CompletingRow_ClearsAndScores()
        {
            var dealer = new ScriptedDealer("##", "#", "#", "#");
            var sim = new Sim(TestHelpers.SmallConfig(3, 3, 3), 1, dealer, null);
            sim.Board.SetCell(new GridPos(2, 0), Cell.Block);

            CommandResult result = sim.Execute(new PlacePieceCommand(0, new GridPos(0, 0)));

            LinesClearedEvent cleared = TestHelpers.Single<LinesClearedEvent>(result.Events);
            Assert.That(cleared.Lines.Rows, Is.EqualTo(new[] { 0 }));
            Assert.That(cleared.ClearedCells.Count, Is.EqualTo(3));
            Assert.That(sim.Board.IsEmpty(new GridPos(2, 0)), Is.True);

            // 2 cells + 10 for one line.
            Assert.That(sim.Score, Is.EqualTo(12));
            Assert.That(sim.Combo, Is.EqualTo(1));
        }

        [Test]
        public void Place_EventsComeInTurnOrder()
        {
            var dealer = new ScriptedDealer("###", "#", "#", "#");
            var sim = new Sim(TestHelpers.SmallConfig(3, 3, 3), 1, dealer, null);

            CommandResult result = sim.Execute(new PlacePieceCommand(0, new GridPos(0, 0)));

            Assert.That(result.Events[0], Is.TypeOf<PiecePlacedEvent>());
            Assert.That(result.Events[1], Is.TypeOf<LinesClearedEvent>());
            Assert.That(result.Events[2], Is.TypeOf<ScoreChangedEvent>());
            Assert.That(result.Events[3], Is.TypeOf<ComboChangedEvent>());
            Assert.That(result.Events[4], Is.TypeOf<PieceDealtEvent>());
        }

        [Test]
        public void Place_NoClearAfterClear_ResetsCombo()
        {
            var dealer = new ScriptedDealer("###", "#", "#", "#", "#");
            var sim = new Sim(TestHelpers.SmallConfig(3, 3, 3), 1, dealer, null);

            sim.Execute(new PlacePieceCommand(0, new GridPos(0, 0)));
            Assert.That(sim.Combo, Is.EqualTo(1));

            CommandResult result = sim.Execute(new PlacePieceCommand(1, new GridPos(0, 0)));

            Assert.That(sim.Combo, Is.EqualTo(0));
            Assert.That(TestHelpers.Single<ComboChangedEvent>(result.Events).Previous, Is.EqualTo(1));
        }

        #endregion

        #region Game over

        [Test]
        public void GameOver_WhenNoPieceFits()
        {
            // 3x3 board, hand of 1, only 2x2 squares. After one square in the corner, no other fits.
            var dealer = new ScriptedDealer("##/##", "##/##");
            var sim = new Sim(TestHelpers.SmallConfig(3, 3, 1), 1, dealer, null);

            CommandResult result = sim.Execute(new PlacePieceCommand(0, new GridPos(0, 0)));

            Assert.That(result.Success, Is.True);
            Assert.That(sim.IsGameOver, Is.True);
            Assert.That(result.Events[result.Events.Count - 1], Is.TypeOf<GameOverEvent>());
        }

        [Test]
        public void GameOver_RejectsFurtherCommands()
        {
            var dealer = new ScriptedDealer("##/##", "##/##");
            var sim = new Sim(TestHelpers.SmallConfig(3, 3, 1), 1, dealer, null);
            sim.Execute(new PlacePieceCommand(0, new GridPos(0, 0)));

            CommandResult result = sim.Execute(new PlacePieceCommand(0, new GridPos(2, 2)));

            Assert.That(result.Success, Is.False);
        }

        #endregion

        #region Determinism

        [Test]
        public void SameSeedAndCommands_ProduceIdenticalGames()
        {
            List<string> first = PlayGreedy(seed: 99, maxMoves: 200);
            List<string> second = PlayGreedy(seed: 99, maxMoves: 200);

            Assert.That(first, Is.EqualTo(second));
            Assert.That(first.Count, Is.GreaterThan(1));
        }

        [Test]
        public void DifferentSeeds_DealDifferentHands()
        {
            var a = new Sim(DefaultSimConfig.Create(), 1);
            var b = new Sim(DefaultSimConfig.Create(), 2);

            string handA = a.Hand.Get(0).Id + a.Hand.Get(1).Id + a.Hand.Get(2).Id;
            string handB = b.Hand.Get(0).Id + b.Hand.Get(1).Id + b.Hand.Get(2).Id;

            Assert.That(handA, Is.Not.EqualTo(handB));
        }

        /// <summary>
        /// Plays the first valid move each turn and records a trace of what happened.
        /// </summary>
        private static List<string> PlayGreedy(ulong seed, int maxMoves)
        {
            var sim = new Sim(DefaultSimConfig.Create(), seed);
            var trace = new List<string>();

            for (int move = 0; move < maxMoves && !sim.IsGameOver; move++)
            {
                ISimCommand command = FindFirstValidMove(sim);
                CommandResult result = sim.Execute(command);

                Assert.That(result.Success, Is.True);
                trace.Add($"{command}: score {sim.Score}, combo {sim.Combo}, rng {sim.Random.State}");
            }

            trace.Add($"end: score {sim.Score}, over {sim.IsGameOver}");
            return trace;
        }

        private static ISimCommand FindFirstValidMove(Sim sim)
        {
            for (int slot = 0; slot < sim.Hand.Size; slot++)
            {
                PieceShape piece = sim.Hand.Get(slot);

                for (int y = 0; y < sim.Board.Height; y++)
                {
                    for (int x = 0; x < sim.Board.Width; x++)
                    {
                        var origin = new GridPos(x, y);

                        if (sim.Board.CanPlace(piece, origin))
                            return new PlacePieceCommand(slot, origin);
                    }
                }
            }

            Assert.Fail("No valid move although the game is not over.");
            return null;
        }

        #endregion
    }
}
