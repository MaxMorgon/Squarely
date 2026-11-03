using System;
using System.Collections.Generic;

namespace Squarely.Simulation.Tests
{
    /// <summary>
    /// Deals pieces from a fixed list, in order, so tests can script exact scenarios.
    /// </summary>
    public sealed class ScriptedDealer : IPieceDealer
    {
        public const string Id = "scripted";

        private readonly Queue<PieceShape> _pieces;

        public ScriptedDealer(params string[] patterns)
        {
            _pieces = new Queue<PieceShape>();

            for (int i = 0; i < patterns.Length; i++)
                _pieces.Enqueue(PieceShape.FromPattern($"scripted_{i}", patterns[i]));
        }

        public PieceShape Deal(IReadOnlyBoard board, IReadOnlyHand hand, DeterministicRandom random)
        {
            if (_pieces.Count == 0)
                throw new InvalidOperationException("ScriptedDealer ran out of pieces.");

            return _pieces.Dequeue();
        }
    }
}
