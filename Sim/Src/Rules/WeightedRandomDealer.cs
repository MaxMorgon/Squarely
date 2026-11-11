using System;
using System.Collections.Generic;

namespace Squarely.Simulation
{
    /// <summary>
    /// Deals pieces at random, weighted by each definition's Weight.
    /// With <see cref="DealingConfig.GuaranteeHelpfulPiece"/> on, a hand with no helpful piece gets one:
    /// a slot picked at random is redealt from the helpful pieces only, still by weight.
    /// </summary>
    public sealed class WeightedRandomDealer : IPieceDealer
    {
        private readonly List<PieceShape> _shapes = new List<PieceShape>();
        private readonly List<int> _weights = new List<int>();
        private readonly bool _guaranteeHelpfulPiece;

        public WeightedRandomDealer(IEnumerable<PieceDefinition> pieces, DealingConfig dealing)
        {
            if (pieces == null)
                throw new ArgumentNullException(nameof(pieces));

            if (dealing == null)
                throw new ArgumentNullException(nameof(dealing));

            int totalWeight = 0;

            foreach (PieceDefinition piece in pieces)
            {
                if (piece.Weight <= 0)
                    continue;

                _shapes.Add(PieceShape.FromPattern(piece.Id, piece.Pattern, piece.CellTypeId));
                _weights.Add(piece.Weight);
                totalWeight += piece.Weight;
            }

            if (totalWeight == 0)
                throw new ArgumentException("Dealer needs at least one piece with positive weight.", nameof(pieces));

            _guaranteeHelpfulPiece = dealing.GuaranteeHelpfulPiece;
        }

        public IReadOnlyList<PieceShape> DealHand(Sim sim, DeterministicRandom random)
        {
            var picks = new List<int>(sim.Hand.Size);

            for (int slot = 0; slot < sim.Hand.Size; slot++)
                picks.Add(PickWeighted(random, null));

            if (_guaranteeHelpfulPiece)
                EnsureHelpfulPiece(sim, random, picks);

            var hand = new List<PieceShape>(picks.Count);

            foreach (int index in picks)
                hand.Add(_shapes[index]);

            return hand;
        }

        #region Helpful piece guarantee

        /// <summary>Redeals one random slot from the helpful pieces if none of the picks is helpful. Draws no random numbers otherwise.</summary>
        private void EnsureHelpfulPiece(Sim sim, DeterministicRandom random, List<int> picks)
        {
            bool[] helpful = FindHelpfulPieces(sim);

            if (helpful == null)
                return;

            foreach (int index in picks)
            {
                if (helpful[index])
                    return;
            }

            int slot = random.NextInt(picks.Count);
            picks[slot] = PickWeighted(random, helpful);
        }

        /// <summary>
        /// Per pool piece: whether it can complete a pattern; if no piece can, whether it fits. Null if no piece fits.
        /// </summary>
        private bool[] FindHelpfulPieces(Sim sim)
        {
            Board scratch = sim.EditableBoard.Clone();
            var clears = new bool[_shapes.Count];
            var fits = new bool[_shapes.Count];
            bool anyClears = false;
            bool anyFits = false;

            for (int i = 0; i < _shapes.Count; i++)
            {
                fits[i] = scratch.CanPlaceAnywhere(_shapes[i]);

                if (!fits[i])
                    continue;

                anyFits = true;
                clears[i] = CanComplete(scratch, _shapes[i], sim.Patterns);
                anyClears |= clears[i];
            }

            if (anyClears)
                return clears;

            if (anyFits)
                return fits;

            return null;
        }

        /// <summary>Whether placing the shape somewhere completes at least one pattern. Leaves the board as it was.</summary>
        private static bool CanComplete(Board board, PieceShape shape, IReadOnlyList<BoardPattern> patterns)
        {
            for (int y = 0; y <= board.Height - shape.Height; y++)
            {
                for (int x = 0; x <= board.Width - shape.Width; x++)
                {
                    var origin = new GridPos(x, y);

                    if (!board.CanPlace(shape, origin))
                        continue;

                    board.Place(shape, origin);
                    bool completes = PatternMatcher.FindMatches(board, patterns).Count > 0;

                    foreach (GridPos offset in shape.Offsets)
                        board.SetCell(origin + offset, Cell.Empty);

                    if (completes)
                        return true;
                }
            }

            return false;
        }

        #endregion

        /// <summary>A pool index by weight, among the allowed pieces only (all when null).</summary>
        private int PickWeighted(DeterministicRandom random, bool[] allowed)
        {
            int totalWeight = 0;

            for (int i = 0; i < _shapes.Count; i++)
            {
                if (allowed == null || allowed[i])
                    totalWeight += _weights[i];
            }

            int roll = random.NextInt(totalWeight);

            for (int i = 0; i < _shapes.Count; i++)
            {
                if (allowed != null && !allowed[i])
                    continue;

                if (roll < _weights[i])
                    return i;

                roll -= _weights[i];
            }

            // Unreachable: roll is always below the total weight.
            throw new InvalidOperationException("Weighted roll out of range.");
        }
    }
}
