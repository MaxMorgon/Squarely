using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Squarely.Simulation.Tests
{
    public class SimPreviewTests
    {
        #region Same result as Execute

        [Test]
        public void Preview_MatchesExecute_ForEveryMoveOfAFullGame()
        {
            var sim = new Sim(DefaultSimConfig.Create(), 7);
            bool sawClear = false;

            for (int move = 0; move < 200 && !sim.IsResolved; move++)
            {
                PlacePieceCommand command = TestHelpers.FirstValidMove(sim);

                List<string> preview = TestHelpers.Describe(sim.Preview(command));
                CommandResult executed = sim.Execute(command);

                Assert.That(preview, Is.EqualTo(TestHelpers.Describe(executed)), $"Move {move}: {command}");
                sawClear |= HasEvent<PatternsClearedEvent>(executed);
            }

            Assert.That(sim.IsResolved, Is.True, "The game should run to its end, so resolution is covered too.");
            Assert.That(sawClear, Is.True, "The game should include clears.");
        }

        [Test]
        public void Preview_InvalidCommand_FailsLikeExecute()
        {
            var dealer = new ScriptedDealer("###", "#", "#");
            Sim sim = TestHelpers.CreateSim(TestHelpers.SmallConfig(8, 8, 3), dealer);
            var command = new PlacePieceCommand(TestHelpers.SlotInstanceId(sim, 0), new GridPos(7, 0));

            CommandResult preview = sim.Preview(command);

            Assert.That(preview.Success, Is.False);
            Assert.That(TestHelpers.Describe(preview), Is.EqualTo(TestHelpers.Describe(sim.Execute(command))));
        }

        [Test]
        public void Preview_RepeatedCalls_ReturnSameResult()
        {
            var sim = new Sim(DefaultSimConfig.Create(), 3);
            PlacePieceCommand command = TestHelpers.FirstValidMove(sim);

            List<string> first = TestHelpers.Describe(sim.Preview(command));
            List<string> second = TestHelpers.Describe(sim.Preview(command));

            Assert.That(second, Is.EqualTo(first));
        }

        #endregion

        #region Real sim untouched

        [Test]
        public void Preview_ClearingMove_DoesNotChangeState()
        {
            var dealer = new ScriptedDealer("##", "#", "#", "#");
            Sim sim = TestHelpers.CreateSim(TestHelpers.SmallConfig(3, 3, 3), dealer);
            sim.EditableBoard.SetCell(new GridPos(2, 0), Cell.Block);
            List<string> before = TestHelpers.DescribeState(sim);

            CommandResult preview = sim.Preview(new PlacePieceCommand(TestHelpers.SlotInstanceId(sim, 0), new GridPos(0, 0)));

            Assert.That(HasEvent<PatternsClearedEvent>(preview), Is.True);
            Assert.That(HasEvent<PieceDealtEvent>(preview), Is.True);
            Assert.That(TestHelpers.DescribeState(sim), Is.EqualTo(before));
        }

        [Test]
        public void Preview_DoesNotUseUpDealtPieces()
        {
            var dealer = new ScriptedDealer("#", "#", "#", "##/##");
            Sim sim = TestHelpers.CreateSim(TestHelpers.SmallConfig(8, 8, 3), dealer);
            var command = new PlacePieceCommand(TestHelpers.SlotInstanceId(sim, 0), new GridPos(0, 0));

            PieceDealtEvent previewDealt = TestHelpers.Single<PieceDealtEvent>(sim.Preview(command).Events);
            PieceDealtEvent executedDealt = TestHelpers.Single<PieceDealtEvent>(sim.Execute(command).Events);

            Assert.That(previewDealt.Piece.Shape.Id, Is.EqualTo("scripted_3"));
            Assert.That(executedDealt.Piece.Shape.Id, Is.EqualTo("scripted_3"));
            Assert.That(executedDealt.Piece.InstanceId, Is.EqualTo(previewDealt.Piece.InstanceId));
        }

        [Test]
        public void Preview_ResolvingMove_LeavesRealSimInProgress()
        {
            // 3x3 board, hand of 1, only 2x2 squares. After one square in the corner, no other fits.
            var dealer = new ScriptedDealer("##/##", "##/##");
            Sim sim = TestHelpers.CreateSim(TestHelpers.SmallConfig(3, 3, 1), dealer);

            CommandResult preview = sim.Preview(new PlacePieceCommand(TestHelpers.SlotInstanceId(sim, 0), new GridPos(0, 0)));

            Assert.That(TestHelpers.Single<GameResolvedEvent>(preview.Events).Resolution.Outcome, Is.EqualTo(GameOutcome.Lost));
            Assert.That(sim.IsResolved, Is.False);
        }

        #endregion

        #region Clone

        [Test]
        public void Clone_CanPlayMovesAhead_WithoutChangingOriginal()
        {
            var sim = new Sim(DefaultSimConfig.Create(), 5);
            List<string> before = TestHelpers.DescribeState(sim);
            Sim clone = sim.Clone();

            for (int move = 0; move < 3; move++)
                Assert.That(clone.Execute(TestHelpers.FirstValidMove(clone)).Success, Is.True);

            Assert.That(TestHelpers.DescribeState(sim), Is.EqualTo(before));
            Assert.That(TestHelpers.DescribeState(clone), Is.Not.EqualTo(before));
        }

        [Test]
        public void Clone_PlaysOutTheSameGame_AsOriginal()
        {
            var sim = new Sim(DefaultSimConfig.Create(), 5);
            Sim clone = sim.Clone();

            for (int move = 0; move < 20 && !sim.IsResolved; move++)
            {
                PlacePieceCommand command = TestHelpers.FirstValidMove(sim);

                Assert.That(TestHelpers.Describe(clone.Execute(command)), Is.EqualTo(TestHelpers.Describe(sim.Execute(command))));
            }

            Assert.That(TestHelpers.DescribeState(clone), Is.EqualTo(TestHelpers.DescribeState(sim)));
        }

        #endregion

        #region Edge cases

        [Test]
        public void Preview_OnResolvedSim_Fails()
        {
            var dealer = new ScriptedDealer("##/##", "##/##");
            Sim sim = TestHelpers.CreateSim(TestHelpers.SmallConfig(3, 3, 1), dealer);
            sim.Execute(new PlacePieceCommand(TestHelpers.SlotInstanceId(sim, 0), new GridPos(0, 0)));

            CommandResult preview = sim.Preview(new PlacePieceCommand(TestHelpers.SlotInstanceId(sim, 0), new GridPos(2, 2)));

            Assert.That(preview.Success, Is.False);
        }

        [Test]
        public void Preview_NullCommand_Throws()
        {
            var sim = new Sim(DefaultSimConfig.Create(), 1);

            Assert.Throws<ArgumentNullException>(() => sim.Preview(null));
        }

        #endregion

        private static bool HasEvent<T>(CommandResult result) where T : ISimEvent
        {
            foreach (ISimEvent simEvent in result.Events)
            {
                if (simEvent is T)
                    return true;
            }

            return false;
        }
    }
}
