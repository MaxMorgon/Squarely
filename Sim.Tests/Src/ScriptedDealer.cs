using System;
using System.Collections.Generic;

namespace Squarely.Simulation.Tests
{
    /// <summary>
    /// Deals pieces from a fixed list, in order, so tests can script exact scenarios.
    /// Stateless like every rule: the next piece is chosen by <see cref="Sim.PiecesDealt"/>.
    /// </summary>
    public sealed class ScriptedDealer : IPieceDealer
    {
        public const string Id = "scripted";

        private readonly List<PieceShape> _pieces;

        public ScriptedDealer(params string[] patterns)
        {
            _pieces = new List<PieceShape>();

            for (int i = 0; i < patterns.Length; i++)
                _pieces.Add(PieceShape.FromPattern($"scripted_{i}", patterns[i]));
        }

        public PieceShape Deal(Sim sim, DeterministicRandom random)
        {
            if (sim.PiecesDealt >= _pieces.Count)
                throw new InvalidOperationException("ScriptedDealer ran out of pieces.");

            return _pieces[sim.PiecesDealt];
        }
    }
}
