using System.Collections.Generic;
using NUnit.Framework;

namespace Squarely.Simulation.Tests
{
    public class ReplayRunnerTests
    {
        [Test]
        public void Run_RecordedGame_Matches()
        {
            GameReplay replay = ReplayTestHelpers.RecordGame(DefaultSimConfig.Create(), 11);

            ReplayRunResult result = ReplayRunner.Run(replay, DefaultSimConfig.Create());

            Assert.That(result.FingerprintMatches, Is.True);
            Assert.That(result.FirstFailedIndex, Is.EqualTo(-1));
            Assert.That(result.EndStateMatches, Is.True);
            Assert.That(result.EndScore, Is.EqualTo(replay.EndScore));
            Assert.That(result.EndIsResolved, Is.True);
        }

        [Test]
        public void Run_HalfFinishedGame_Matches()
        {
            GameReplay replay = ReplayTestHelpers.RecordGame(DefaultSimConfig.Create(), 11, maxMoves: 10);

            ReplayRunResult result = ReplayRunner.Run(replay, DefaultSimConfig.Create());

            Assert.That(replay.EndIsResolved, Is.False);
            Assert.That(result.EndStateMatches, Is.True);
        }

        [Test]
        public void Run_ChangedConfig_ReportsFingerprintMismatch_AndStillRuns()
        {
            GameReplay replay = ReplayTestHelpers.RecordGame(DefaultSimConfig.Create(), 11);
            SimConfig changed = DefaultSimConfig.Create();
            changed.Scoring.PointsPerCellPlaced += 1;

            ReplayRunResult result = ReplayRunner.Run(replay, changed);

            Assert.That(result.FingerprintMatches, Is.False);
            Assert.That(result.FirstFailedIndex, Is.EqualTo(-1), "Only the scoring changed, so the moves still work.");
            Assert.That(result.EndStateMatches, Is.False);
        }

        [Test]
        public void Run_TamperedCommand_ReportsFirstFailedIndex()
        {
            GameReplay replay = ReplayTestHelpers.RecordGame(DefaultSimConfig.Create(), 11);
            var commands = new List<ISimCommand>(replay.Commands);
            commands[5] = new PlacePieceCommand(9999, new GridPos(0, 0));
            commands[8] = new PlacePieceCommand(9999, new GridPos(0, 0));
            var tampered = new GameReplay(replay.Seed, replay.ConfigFingerprint, commands, replay.EndScore, replay.EndIsResolved);

            ReplayRunResult result = ReplayRunner.Run(tampered, DefaultSimConfig.Create());

            Assert.That(result.FirstFailedIndex, Is.EqualTo(5));
            Assert.That(result.FailureReason, Is.Not.Empty);
            Assert.That(result.AllCommandsSucceeded, Is.False);
            Assert.That(result.EndStateMatches, Is.False);
        }

        [Test]
        public void Run_ReplayWithEndGame_Matches()
        {
            GameReplay replay = ReplayTestHelpers.RecordGame(DefaultSimConfig.Create(), 11, maxMoves: 6, endWithEndGame: true);

            ReplayRunResult result = ReplayRunner.Run(replay, DefaultSimConfig.Create());

            Assert.That(replay.EndIsResolved, Is.True);
            Assert.That(result.EndStateMatches, Is.True);
        }

        [Test]
        public void CreateSim_StartsAtTheReplaySeed_AndStepsLikeRun()
        {
            GameReplay replay = ReplayTestHelpers.RecordGame(DefaultSimConfig.Create(), 11);

            Sim sim = ReplayRunner.CreateSim(replay, DefaultSimConfig.Create());

            Assert.That(sim.Seed, Is.EqualTo(replay.Seed));
            Assert.That(sim.Score, Is.EqualTo(0));

            foreach (ISimCommand command in replay.Commands)
                Assert.That(sim.Execute(command).Success, Is.True);

            Assert.That(sim.Score, Is.EqualTo(replay.EndScore));
        }

        /// <summary>The determinism contract: record, encode, decode and run gives the same end state.</summary>
        [Test]
        public void Scenario_RecordEncodeDecodeRun_GivesSameEndState()
        {
            for (ulong seed = 1; seed <= 20; seed++)
            {
                GameReplay recorded = ReplayTestHelpers.RecordGame(DefaultSimConfig.Create(), seed, endWithEndGame: seed % 2 == 0);

                ReplayDecodeResult decoded = ReplayCodec.Decode(ReplayCodec.Encode(recorded));
                ReplayRunResult result = ReplayRunner.Run(decoded.Replay, DefaultSimConfig.Create());

                Assert.That(decoded.Success, Is.True, $"Seed {seed}");
                Assert.That(result.FingerprintMatches, Is.True, $"Seed {seed}");
                Assert.That(result.AllCommandsSucceeded, Is.True, $"Seed {seed}");
                Assert.That(result.EndStateMatches, Is.True, $"Seed {seed}");
                Assert.That(result.EndScore, Is.EqualTo(recorded.EndScore), $"Seed {seed}");
            }
        }
    }
}
