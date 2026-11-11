using System.Collections.Generic;
using NUnit.Framework;

namespace Squarely.Simulation.Tests
{
    /// <summary>
    /// A sim keeps its own copy of the config: edits to the config object after construction change nothing in it.
    /// </summary>
    public class SimConfigCopyTests
    {
        [Test]
        public void EditPieces_AfterConstruction_DoesNotChangeDeals()
        {
            SimConfig config = TestHelpers.SmallConfig(8, 8, 1);
            var sim = new Sim(config, 1);

            config.Pieces[0].Id = "edited";
            config.Pieces[0].Pattern = "##";
            config.Pieces.Add(new PieceDefinition("big", "###", 1000));

            var dealt = new List<PieceDealtEvent>();

            for (int move = 0; move < 10; move++)
            {
                CommandResult result = sim.Execute(TestHelpers.FirstValidMove(sim));
                dealt.AddRange(TestHelpers.All<PieceDealtEvent>(result.Events));
            }

            Assert.That(dealt.Count, Is.EqualTo(10));

            foreach (PieceDealtEvent dealtEvent in dealt)
                Assert.That(dealtEvent.Piece.Shape.Id, Is.EqualTo("dot"));
        }

        [Test]
        public void EditClearing_AfterConstruction_DoesNotChangePatterns()
        {
            SimConfig config = TestHelpers.SmallConfig(3, 3, 1);
            config.Clearing.Patterns.Add(new BoardPatternDefinition { Id = "corner", Pattern = "##/#.", Points = 20 });
            Sim sim = TestHelpers.CreateSim(config, new ScriptedDealer("###", "#"));

            config.Clearing.ClearRows = false;
            config.Clearing.LinePoints = 999;
            config.Clearing.Patterns[0].Points = 999;
            config.Clearing.Patterns.Add(new BoardPatternDefinition { Id = "added", Pattern = "#", Points = 500 });

            Assert.That(sim.Patterns.Count, Is.EqualTo(3 + 3 + 1));
            Assert.That(sim.Patterns[0].Points, Is.EqualTo(10));
            Assert.That(sim.Patterns[6].Id, Is.EqualTo("corner"));
            Assert.That(sim.Patterns[6].Points, Is.EqualTo(20));

            CommandResult result = sim.Execute(new PlacePieceCommand(TestHelpers.SlotInstanceId(sim, 0), new GridPos(0, 0)));

            Assert.That(TestHelpers.Single<PatternsClearedEvent>(result.Events).ClearedCells.Count, Is.EqualTo(3));
            Assert.That(TestHelpers.Single<ScoreChangedEvent>(result.Events).Delta, Is.EqualTo(3 + 10));
        }

        [Test]
        public void EditScoring_AfterConstruction_DoesNotChangeScore()
        {
            SimConfig config = TestHelpers.SmallConfig(3, 3, 1);
            Sim sim = TestHelpers.CreateSim(config, new ScriptedDealer("###", "#"));

            config.Scoring.PointsPerCellPlaced = 100;
            config.Scoring.MultiMatchBonusPerExtra = 100;
            config.Scoring.ComboBonusPercent = 100;

            CommandResult result = sim.Execute(new PlacePieceCommand(TestHelpers.SlotInstanceId(sim, 0), new GridPos(0, 0)));

            Assert.That(TestHelpers.Single<ScoreChangedEvent>(result.Events).Delta, Is.EqualTo(3 + 10));
        }

        [Test]
        public void EditBoardAndHandSize_AfterConstruction_DoesNotResizeSim()
        {
            SimConfig config = TestHelpers.SmallConfig(3, 3, 1);
            var sim = new Sim(config, 1);

            config.BoardWidth = 10;
            config.BoardHeight = 10;
            config.HandSize = 5;

            Assert.That(sim.Board.Width, Is.EqualTo(3));
            Assert.That(sim.Board.Height, Is.EqualTo(3));
            Assert.That(sim.Hand.Size, Is.EqualTo(1));
        }

        [Test]
        public void EditConfig_AfterConstruction_DoesNotChangePreviews()
        {
            SimConfig config = TestHelpers.SmallConfig(3, 3, 1);
            Sim sim = TestHelpers.CreateSim(config, new ScriptedDealer("###", "#"));

            config.Clearing.LinePoints = 999;
            config.Scoring.PointsPerCellPlaced = 100;

            CommandResult result = sim.Preview(new PlacePieceCommand(TestHelpers.SlotInstanceId(sim, 0), new GridPos(0, 0)));

            Assert.That(TestHelpers.Single<ScoreChangedEvent>(result.Events).Delta, Is.EqualTo(3 + 10));
        }
    }
}
