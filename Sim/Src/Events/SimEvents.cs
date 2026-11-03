using System.Collections.Generic;

namespace Squarely.Simulation
{
    /// <summary>
    /// Something that happened in the sim. Commands return these in the order they happened.
    /// </summary>
    public interface ISimEvent
    {
    }

    public sealed class PiecePlacedEvent : ISimEvent
    {
        public PiecePlacedEvent(int slot, PieceShape piece, GridPos origin)
        {
            Slot = slot;
            Piece = piece;
            Origin = origin;
        }

        public int Slot { get; }

        public PieceShape Piece { get; }

        public GridPos Origin { get; }
    }

    public sealed class LinesClearedEvent : ISimEvent
    {
        public LinesClearedEvent(BoardLines lines, IReadOnlyList<GridPos> clearedCells)
        {
            Lines = lines;
            ClearedCells = clearedCells;
        }

        public BoardLines Lines { get; }

        public IReadOnlyList<GridPos> ClearedCells { get; }
    }

    public sealed class ScoreChangedEvent : ISimEvent
    {
        public ScoreChangedEvent(int delta, int total)
        {
            Delta = delta;
            Total = total;
        }

        public int Delta { get; }

        public int Total { get; }
    }

    public sealed class ComboChangedEvent : ISimEvent
    {
        public ComboChangedEvent(int previous, int current)
        {
            Previous = previous;
            Current = current;
        }

        public int Previous { get; }

        public int Current { get; }
    }

    public sealed class PieceDealtEvent : ISimEvent
    {
        public PieceDealtEvent(int slot, PieceShape piece)
        {
            Slot = slot;
            Piece = piece;
        }

        public int Slot { get; }

        public PieceShape Piece { get; }
    }

    public sealed class GameOverEvent : ISimEvent
    {
        public GameOverEvent(int finalScore)
        {
            FinalScore = finalScore;
        }

        public int FinalScore { get; }
    }
}
