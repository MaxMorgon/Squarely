using System;
using NUnit.Framework;

namespace Squarely.Simulation.Tests
{
    public class PieceShapeTests
    {
        [Test]
        public void FromPattern_SingleCell_IsOneByOne()
        {
            PieceShape shape = PieceShape.FromPattern("dot", "#");

            Assert.That(shape.CellCount, Is.EqualTo(1));
            Assert.That(shape.Width, Is.EqualTo(1));
            Assert.That(shape.Height, Is.EqualTo(1));
            Assert.That(shape.Offsets[0], Is.EqualTo(new GridPos(0, 0)));
        }

        [Test]
        public void FromPattern_TopRowFirst_MapsToBoardYUp()
        {
            // #.
            // ##
            PieceShape shape = PieceShape.FromPattern("corner", "#./##");

            Assert.That(shape.Offsets, Is.EquivalentTo(new[]
            {
                new GridPos(0, 1),
                new GridPos(0, 0),
                new GridPos(1, 0),
            }));
            Assert.That(shape.Width, Is.EqualTo(2));
            Assert.That(shape.Height, Is.EqualTo(2));
        }

        [Test]
        public void FromPattern_EmptyBorder_IsNormalizedAway()
        {
            PieceShape shape = PieceShape.FromPattern("padded", "../.#");

            Assert.That(shape.Offsets, Is.EquivalentTo(new[] { new GridPos(0, 0) }));
            Assert.That(shape.Width, Is.EqualTo(1));
            Assert.That(shape.Height, Is.EqualTo(1));
        }

        [Test]
        public void FromPattern_InvalidCharacter_Throws()
        {
            Assert.Throws<FormatException>(() => PieceShape.FromPattern("bad", "#x"));
        }

        [Test]
        public void FromPattern_NoFilledCells_Throws()
        {
            Assert.Throws<ArgumentException>(() => PieceShape.FromPattern("empty", "../.."));
        }

        [Test]
        public void FromPattern_DefaultCellType_IsBlock()
        {
            Assert.That(PieceShape.FromPattern("dot", "#").CellTypeId, Is.EqualTo(Cell.BlockType));
            Assert.That(PieceShape.FromPattern("dot", "#", "stone").CellTypeId, Is.EqualTo("stone"));
        }

        [Test]
        public void Constructor_DuplicateOffsets_AreRemoved()
        {
            var shape = new PieceShape("dupes", new[] { new GridPos(2, 3), new GridPos(2, 3) });

            Assert.That(shape.CellCount, Is.EqualTo(1));
            Assert.That(shape.Offsets[0], Is.EqualTo(new GridPos(0, 0)));
        }

        [Test]
        public void DefaultConfig_AllPatterns_Parse()
        {
            foreach (PieceDefinition piece in DefaultSimConfig.Create().Pieces)
                Assert.DoesNotThrow(() => PieceShape.FromPattern(piece.Id, piece.Pattern), piece.Id);
        }
    }
}
