using System;
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
                Assert.That(sim.Hand.IsSlotEmpty(slot), Is.False);

            Assert.That(sim.Score, Is.EqualTo(0));
            Assert.That(sim.Combo, Is.EqualTo(0));
            Assert.That(sim.IsResolved, Is.False);
            Assert.That(sim.Patterns.Count, Is.EqualTo(16));
        }

        [Test]
        public void NewSim_InstanceIdsAreUnique_InDealOrder()
        {
            var sim = new Sim(DefaultSimConfig.Create(), 1);

            Assert.That(TestHelpers.SlotInstanceId(sim, 0), Is.EqualTo(1));
            Assert.That(TestHelpers.SlotInstanceId(sim, 1), Is.EqualTo(2));
            Assert.That(TestHelpers.SlotInstanceId(sim, 2), Is.EqualTo(3));
        }

        [Test]
        public void UnknownRuleId_Throws()
        {
            SimConfig config = DefaultSimConfig.Create();
            config.ScoringRuleId = "does_not_exist";

            Assert.Throws<InvalidOperationException>(() => new Sim(config, 1));
        }

        #endregion

        #region Placement

        [Test]
        public void Place_ValidMove_Succeeds_AndRefillsSameSlot()
        {
            var dealer = new ScriptedDealer("#", "##", "###", "#/#");
            Sim sim = TestHelpers.CreateSim(TestHelpers.SmallConfig(8, 8, 3), dealer);
            int firstId = TestHelpers.SlotInstanceId(sim, 0);

            CommandResult result = sim.Execute(new PlacePieceCommand(firstId, new GridPos(0, 0)));

            Assert.That(result.Success, Is.True);
            Assert.That(sim.Board.IsEmpty(new GridPos(0, 0)), Is.False);
            Assert.That(sim.Hand.FindSlot(firstId), Is.EqualTo(-1));
            Assert.That(sim.Hand.GetSlot(0).Shape.Id, Is.EqualTo("scripted_3"));
            Assert.That(sim.Hand.GetSlot(0).InstanceId, Is.EqualTo(4));

            PieceDealtEvent dealt = TestHelpers.Single<PieceDealtEvent>(result.Events);
            Assert.That(dealt.Slot, Is.EqualTo(0));
        }

        [Test]
        public void Place_IdenticalShapes_AreToldApartByInstanceId()
        {
            var dealer = new ScriptedDealer("#", "#", "#", "#");
            Sim sim = TestHelpers.CreateSim(TestHelpers.SmallConfig(8, 8, 3), dealer);
            int secondId = TestHelpers.SlotInstanceId(sim, 1);

            CommandResult result = sim.Execute(new PlacePieceCommand(secondId, new GridPos(4, 4)));

            Assert.That(TestHelpers.Single<PiecePlacedEvent>(result.Events).Slot, Is.EqualTo(1));
            Assert.That(sim.Hand.GetSlot(0).InstanceId, Is.EqualTo(1));
        }

        [Test]
        public void Place_OutOfBounds_FailsWithoutChangingState()
        {
            var dealer = new ScriptedDealer("###", "#", "#");
            Sim sim = TestHelpers.CreateSim(TestHelpers.SmallConfig(8, 8, 3), dealer);
            int id = TestHelpers.SlotInstanceId(sim, 0);
            ulong randomState = sim.RandomState;

            CommandResult result = sim.Execute(new PlacePieceCommand(id, new GridPos(7, 0)));

            Assert.That(result.Success, Is.False);
            Assert.That(result.Events, Is.Empty);
            Assert.That(sim.Hand.FindSlot(id), Is.EqualTo(0));
            Assert.That(sim.Board.IsEmpty(new GridPos(7, 0)), Is.True);
            Assert.That(sim.RandomState, Is.EqualTo(randomState));
        }

        [Test]
        public void Place_UnknownInstanceId_Fails()
        {
            var sim = new Sim(TestHelpers.SmallConfig(8, 8, 3), 1);

            Assert.That(sim.Execute(new PlacePieceCommand(999, new GridPos(0, 0))).Success, Is.False);
        }

        [Test]
        public void Place_AlreadyPlacedInstance_Fails()
        {
            var sim = new Sim(TestHelpers.SmallConfig(8, 8, 3), 1);
            int id = TestHelpers.SlotInstanceId(sim, 0);

            sim.Execute(new PlacePieceCommand(id, new GridPos(0, 0)));

            Assert.That(sim.Execute(new PlacePieceCommand(id, new GridPos(1, 1))).Success, Is.False);
        }

        [Test]
        public void Place_CompletingRow_ClearsAndScores()
        {
            var dealer = new ScriptedDealer("##", "#", "#", "#");
            Sim sim = TestHelpers.CreateSim(TestHelpers.SmallConfig(3, 3, 3), dealer);
            sim.EditableBoard.SetCell(new GridPos(2, 0), Cell.Block);

            CommandResult result = sim.Execute(new PlacePieceCommand(TestHelpers.SlotInstanceId(sim, 0), new GridPos(0, 0)));

            PatternsClearedEvent cleared = TestHelpers.Single<PatternsClearedEvent>(result.Events);
            Assert.That(cleared.Matches.Count, Is.EqualTo(1));
            Assert.That(cleared.Matches[0].Pattern.Id, Is.EqualTo("row_0"));
            Assert.That(cleared.ClearedCells.Count, Is.EqualTo(3));
            Assert.That(sim.Board.IsEmpty(new GridPos(2, 0)), Is.True);

            // 2 cells + 10 for one line.
            Assert.That(sim.Score, Is.EqualTo(12));
            Assert.That(sim.Combo, Is.EqualTo(1));
        }

        [Test]
        public void Place_CompletingCustomPattern_ClearsWithItsPoints()
        {
            SimConfig config = TestHelpers.SmallConfig(5, 5, 1);
            config.Clearing.Patterns.Add(new BoardPatternDefinition { Id = "pair", Pattern = "##", Points = 100 });

            var dealer = new ScriptedDealer("##", "#");
            Sim sim = TestHelpers.CreateSim(config, dealer);

            CommandResult result = sim.Execute(new PlacePieceCommand(TestHelpers.SlotInstanceId(sim, 0), new GridPos(2, 2)));

            PatternsClearedEvent cleared = TestHelpers.Single<PatternsClearedEvent>(result.Events);
            Assert.That(cleared.Matches[0].Pattern.Id, Is.EqualTo("pair"));
            Assert.That(sim.Board.IsEmpty(new GridPos(2, 2)), Is.True);
            Assert.That(sim.Score, Is.EqualTo(2 + 100));
        }

        [Test]
        public void Place_EventsComeInTurnOrder()
        {
            var dealer = new ScriptedDealer("###", "#", "#", "#");
            Sim sim = TestHelpers.CreateSim(TestHelpers.SmallConfig(3, 3, 3), dealer);

            CommandResult result = sim.Execute(new PlacePieceCommand(TestHelpers.SlotInstanceId(sim, 0), new GridPos(0, 0)));

            Assert.That(result.Events[0], Is.TypeOf<PiecePlacedEvent>());
            Assert.That(result.Events[1], Is.TypeOf<PatternsClearedEvent>());
            Assert.That(result.Events[2], Is.TypeOf<ScoreChangedEvent>());
            Assert.That(result.Events[3], Is.TypeOf<ComboChangedEvent>());
            Assert.That(result.Events[4], Is.TypeOf<PieceDealtEvent>());
        }

        [Test]
        public void Place_NoMatchAfterMatch_ResetsCombo()
        {
            var dealer = new ScriptedDealer("###", "#", "#", "#", "#");
            Sim sim = TestHelpers.CreateSim(TestHelpers.SmallConfig(3, 3, 3), dealer);

            sim.Execute(new PlacePieceCommand(TestHelpers.SlotInstanceId(sim, 0), new GridPos(0, 0)));
            Assert.That(sim.Combo, Is.EqualTo(1));

            CommandResult result = sim.Execute(new PlacePieceCommand(TestHelpers.SlotInstanceId(sim, 1), new GridPos(0, 0)));

            Assert.That(sim.Combo, Is.EqualTo(0));
            Assert.That(TestHelpers.Single<ComboChangedEvent>(result.Events).Previous, Is.EqualTo(1));
        }

        #endregion

        #region Resolution

        [Test]
        public void NoMovesLeft_ResolvesAsLost()
        {
            // 3x3 board, hand of 1, only 2x2 squares. After one square in the corner, no other fits.
            var dealer = new ScriptedDealer("##/##", "##/##");
            Sim sim = TestHelpers.CreateSim(TestHelpers.SmallConfig(3, 3, 1), dealer);

            CommandResult result = sim.Execute(new PlacePieceCommand(TestHelpers.SlotInstanceId(sim, 0), new GridPos(0, 0)));

            Assert.That(result.Success, Is.True);
            Assert.That(sim.IsResolved, Is.True);
            Assert.That(sim.Resolution.Outcome, Is.EqualTo(GameOutcome.Lost));
            Assert.That(sim.Resolution.ReasonId, Is.EqualTo(RuleIds.NoMovesLeft));

            var resolved = (GameResolvedEvent)result.Events[result.Events.Count - 1];
            Assert.That(resolved.FinalScore, Is.EqualTo(sim.Score));
        }

        [Test]
        public void Resolved_RejectsFurtherCommands()
        {
            var dealer = new ScriptedDealer("##/##", "##/##");
            Sim sim = TestHelpers.CreateSim(TestHelpers.SmallConfig(3, 3, 1), dealer);
            sim.Execute(new PlacePieceCommand(TestHelpers.SlotInstanceId(sim, 0), new GridPos(0, 0)));

            CommandResult result = sim.Execute(new PlacePieceCommand(TestHelpers.SlotInstanceId(sim, 0), new GridPos(2, 2)));

            Assert.That(result.Success, Is.False);
        }

        [Test]
        public void CustomResolutionRule_FromFactory_CanWin()
        {
            SimConfig config = TestHelpers.SmallConfig(8, 8, 3);
            config.ResolutionRuleIds = new List<string> { "score_target", RuleIds.NoMovesLeft };

            RuleFactory rules = RuleFactory.CreateDefault();
            rules.RegisterResolutionRule("score_target", _ => new ScoreTargetRule(2));
            var sim = new Sim(config, 1, rules);

            sim.Execute(new PlacePieceCommand(TestHelpers.SlotInstanceId(sim, 0), new GridPos(0, 0)));
            Assert.That(sim.IsResolved, Is.False);

            sim.Execute(new PlacePieceCommand(TestHelpers.SlotInstanceId(sim, 1), new GridPos(1, 1)));
            Assert.That(sim.Resolution.Outcome, Is.EqualTo(GameOutcome.Won));
        }

        private sealed class ScoreTargetRule : IResolutionRule
        {
            private readonly int _target;

            public ScoreTargetRule(int target)
            {
                _target = target;
            }

            public GameResolution Check(Sim sim)
            {
                if (sim.Score < _target)
                    return null;

                return new GameResolution(GameOutcome.Won, "score_target");
            }
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

            Assert.That(HandIds(a), Is.Not.EqualTo(HandIds(b)));
        }

        private static string HandIds(Sim sim)
        {
            string ids = "";

            for (int slot = 0; slot < sim.Hand.Size; slot++)
                ids += sim.Hand.GetSlot(slot).Shape.Id + ",";

            return ids;
        }

        /// <summary>
        /// Plays the first valid move each turn and records a trace of what happened.
        /// </summary>
        private static List<string> PlayGreedy(ulong seed, int maxMoves)
        {
            var sim = new Sim(DefaultSimConfig.Create(), seed);
            var trace = new List<string>();

            for (int move = 0; move < maxMoves && !sim.IsResolved; move++)
            {
                ISimCommand command = FindFirstValidMove(sim);
                CommandResult result = sim.Execute(command);

                Assert.That(result.Success, Is.True);
                trace.Add($"{command}: score {sim.Score}, combo {sim.Combo}, rng {sim.RandomState}");
            }

            trace.Add($"end: score {sim.Score}, resolution {sim.Resolution}");
            return trace;
        }

        private static ISimCommand FindFirstValidMove(Sim sim)
        {
            for (int slot = 0; slot < sim.Hand.Size; slot++)
            {
                PieceInstance piece = sim.Hand.GetSlot(slot);

                for (int y = 0; y < sim.Board.Height; y++)
                {
                    for (int x = 0; x < sim.Board.Width; x++)
                    {
                        var origin = new GridPos(x, y);

                        if (sim.Board.CanPlace(piece.Shape, origin))
                            return new PlacePieceCommand(piece.InstanceId, origin);
                    }
                }
            }

            Assert.Fail("No valid move although the game is not resolved.");
            return null;
        }

        #endregion
    }
}
