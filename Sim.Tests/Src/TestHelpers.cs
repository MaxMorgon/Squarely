using System;
using System.Collections.Generic;

namespace Squarely.Simulation.Tests
{
    /// <summary>
    /// Deals pieces from a fixed list, in order, so tests can script exact scenarios.
    /// </summary>
    public sealed class ScriptedDealer : IPieceDealer
    {
        private readonly Queue<PieceShape> _pieces;

        public ScriptedDealer(params string[] patterns)
        {
            _pieces = new Queue<PieceShape>();

            for (int i = 0; i < patterns.Length; i++)
                _pieces.Enqueue(PieceShape.FromPattern($"scripted_{i}", patterns[i]));
        }

        public PieceShape Deal(Board board, Hand hand, DeterministicRandom random)
        {
            if (_pieces.Count == 0)
                throw new InvalidOperationException("ScriptedDealer ran out of pieces.");

            return _pieces.Dequeue();
        }
    }

    public static class TestHelpers
    {
        /// <summary>
        /// Fills cells from a pattern (same format as pieces: '/' rows, top first, '#' filled),
        /// with the pattern's bottom-left at the board's bottom-left.
        /// </summary>
        public static void Fill(Board board, string pattern)
        {
            string[] rows = pattern.Split('/');

            for (int rowIndex = 0; rowIndex < rows.Length; rowIndex++)
            {
                int y = rows.Length - 1 - rowIndex;

                for (int x = 0; x < rows[rowIndex].Length; x++)
                {
                    if (rows[rowIndex][x] == '#')
                        board.SetCell(new GridPos(x, y), Cell.Block);
                }
            }
        }

        public static SimConfig SmallConfig(int width, int height, int handSize)
        {
            return new SimConfig
            {
                BoardWidth = width,
                BoardHeight = height,
                HandSize = handSize,
                Pieces = new List<PieceDefinition> { new PieceDefinition("dot", "#") },
                Scoring = new ScoringConfig(),
            };
        }

        public static T Single<T>(IReadOnlyList<ISimEvent> events) where T : class, ISimEvent
        {
            T found = null;

            foreach (ISimEvent simEvent in events)
            {
                if (!(simEvent is T match))
                    continue;

                if (found != null)
                    throw new InvalidOperationException($"More than one {typeof(T).Name}.");

                found = match;
            }

            if (found == null)
                throw new InvalidOperationException($"No {typeof(T).Name} found.");

            return found;
        }

        public static int Count<T>(IReadOnlyList<ISimEvent> events) where T : ISimEvent
        {
            int count = 0;

            foreach (ISimEvent simEvent in events)
            {
                if (simEvent is T)
                    count++;
            }

            return count;
        }
    }
}
