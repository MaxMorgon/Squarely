using NUnit.Framework;

namespace Squarely.Simulation.Tests
{
    public class GameReplayTests
    {
        [Test]
        public void MatchesEnd_SimAtTheRecordedEnd_IsTrue()
        {
            GameReplay replay = ReplayTestHelpers.RecordGame(DefaultSimConfig.Create(), 11, maxMoves: 10);
            Sim sim = ReplayRunner.CreateSim(replay, DefaultSimConfig.Create());

            foreach (ISimCommand command in replay.Commands)
                sim.Execute(command);

            Assert.That(replay.MatchesEnd(sim), Is.True);
        }

        [Test]
        public void MatchesEnd_SimBeforeTheLastCommand_IsFalse()
        {
            GameReplay replay = ReplayTestHelpers.RecordGame(DefaultSimConfig.Create(), 11, maxMoves: 10);
            Sim sim = ReplayRunner.CreateSim(replay, DefaultSimConfig.Create());

            for (int i = 0; i < replay.Commands.Count - 1; i++)
                sim.Execute(replay.Commands[i]);

            Assert.That(replay.MatchesEnd(sim), Is.False);
        }

        [Test]
        public void MoveCount_CountsPlaceCommandsOnly()
        {
            GameReplay replay = ReplayTestHelpers.RecordGame(DefaultSimConfig.Create(), 11, maxMoves: 6, endWithEndGame: true);

            Assert.That(replay.Commands.Count, Is.EqualTo(7));
            Assert.That(replay.MoveCount, Is.EqualTo(6));
        }
    }
}
