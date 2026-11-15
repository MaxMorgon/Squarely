using NUnit.Framework;

namespace Squarely.Simulation.Tests
{
    public class ReplayRecorderTests
    {
        [Test]
        public void Recorder_Restore_DropsLaterCommandsAndFollowsTheNewSim()
        {
            var sim = new Sim(DefaultSimConfig.Create(), 4);
            var recorder = new ReplayRecorder(sim);
            recorder.Execute(TestHelpers.FirstValidMove(sim));
            Sim afterOne = sim.Clone();
            recorder.Execute(TestHelpers.FirstValidMove(sim));

            recorder.Restore(afterOne, 1);

            Assert.That(recorder.CommandCount, Is.EqualTo(1));
            Assert.That(recorder.Build().EndScore, Is.EqualTo(afterOne.Score));

            recorder.Execute(new EndGameCommand());

            Assert.That(recorder.Build().Commands.Count, Is.EqualTo(2));
            Assert.That(afterOne.IsResolved, Is.True);
        }

        [Test]
        public void Recorder_Restore_RejectsATooLargeCount()
        {
            var sim = new Sim(DefaultSimConfig.Create(), 4);
            var recorder = new ReplayRecorder(sim);

            Assert.Throws<System.ArgumentOutOfRangeException>(() => recorder.Restore(sim.Clone(), 1));
        }

        [Test]
        public void Recorder_SkipsFailedCommands()
        {
            var sim = new Sim(DefaultSimConfig.Create(), 4);
            var recorder = new ReplayRecorder(sim);

            recorder.Execute(new PlacePieceCommand(9999, new GridPos(0, 0)));
            recorder.Execute(TestHelpers.FirstValidMove(sim));
            recorder.Execute(new EndGameCommand());
            recorder.Execute(new EndGameCommand());

            GameReplay replay = recorder.Build();

            Assert.That(replay.Commands.Count, Is.EqualTo(2));
            Assert.That(replay.EndIsResolved, Is.True);
            Assert.That(replay.Seed, Is.EqualTo(4UL));
            Assert.That(replay.ConfigFingerprint, Is.EqualTo(sim.ConfigFingerprint));
        }
    }
}
