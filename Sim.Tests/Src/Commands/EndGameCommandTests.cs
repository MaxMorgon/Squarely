using NUnit.Framework;

namespace Squarely.Simulation.Tests
{
    public class EndGameCommandTests
    {
        [Test]
        public void Execute_EndsGameAsLost_WithNoMovesLeftReason()
        {
            var sim = new Sim(DefaultSimConfig.Create(), 5);

            CommandResult result = sim.Execute(new EndGameCommand());

            Assert.That(result.Success, Is.True);
            Assert.That(sim.IsResolved, Is.True);
            Assert.That(sim.Resolution.Outcome, Is.EqualTo(GameOutcome.Lost));
            Assert.That(sim.Resolution.ReasonId, Is.EqualTo(RuleIds.NoMovesLeft));
        }

        [Test]
        public void Execute_EmitsGameResolvedEvent_WithFinalScore()
        {
            var sim = new Sim(DefaultSimConfig.Create(), 5);
            sim.Execute(TestHelpers.FirstValidMove(sim));

            CommandResult result = sim.Execute(new EndGameCommand());

            GameResolvedEvent resolved = TestHelpers.Single<GameResolvedEvent>(result.Events);
            Assert.That(result.Events.Count, Is.EqualTo(1));
            Assert.That(resolved.FinalScore, Is.EqualTo(sim.Score));
            Assert.That(resolved.Resolution, Is.SameAs(sim.Resolution));
        }

        [Test]
        public void Execute_WhenAlreadyResolved_FailsWithoutChange()
        {
            var sim = new Sim(DefaultSimConfig.Create(), 5);
            sim.Execute(new EndGameCommand());
            GameResolution resolution = sim.Resolution;

            CommandResult result = sim.Execute(new EndGameCommand());

            Assert.That(result.Success, Is.False);
            Assert.That(result.Events, Is.Empty);
            Assert.That(sim.Resolution, Is.SameAs(resolution));
        }

        [Test]
        public void PlacePiece_AfterEndGame_Fails()
        {
            var sim = new Sim(DefaultSimConfig.Create(), 5);
            PlacePieceCommand move = TestHelpers.FirstValidMove(sim);
            sim.Execute(new EndGameCommand());

            Assert.That(sim.Execute(move).Success, Is.False);
        }

        [Test]
        public void Preview_EqualsExecute()
        {
            var sim = new Sim(DefaultSimConfig.Create(), 5);
            var command = new EndGameCommand();

            var preview = TestHelpers.Describe(sim.Preview(command));
            Assert.That(sim.IsResolved, Is.False, "Preview must not change the sim.");

            var executed = TestHelpers.Describe(sim.Execute(command));
            Assert.That(preview, Is.EqualTo(executed));

            Assert.That(TestHelpers.Describe(sim.Preview(command)), Is.EqualTo(TestHelpers.Describe(sim.Execute(command))));
        }
    }
}
