using System;
using System.Collections.Generic;

namespace Squarely.Simulation
{
    /// <summary>
    /// Deals pieces at random, weighted by each definition's Weight.
    /// </summary>
    public sealed class WeightedRandomDealer : IPieceDealer
    {
        private readonly List<PieceShape> _shapes = new List<PieceShape>();
        private readonly List<int> _weights = new List<int>();
        private readonly int _totalWeight;

        public WeightedRandomDealer(IEnumerable<PieceDefinition> pieces)
        {
            if (pieces == null)
                throw new ArgumentNullException(nameof(pieces));

            foreach (PieceDefinition piece in pieces)
            {
                if (piece.Weight <= 0)
                    continue;

                _shapes.Add(PieceShape.FromPattern(piece.Id, piece.Pattern, piece.CellTypeId));
                _weights.Add(piece.Weight);
                _totalWeight += piece.Weight;
            }

            if (_totalWeight == 0)
                throw new ArgumentException("Dealer needs at least one piece with positive weight.", nameof(pieces));
        }

        public PieceShape Deal(Sim sim, DeterministicRandom random)
        {
            int roll = random.NextInt(_totalWeight);

            for (int i = 0; i < _shapes.Count; i++)
            {
                if (roll < _weights[i])
                    return _shapes[i];

                roll -= _weights[i];
            }

            // Unreachable: roll is always below the total weight.
            throw new InvalidOperationException("Weighted roll out of range.");
        }
    }
}
