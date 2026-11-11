using System;
using System.Collections.Generic;

namespace Squarely.Simulation
{
    /// <summary>
    /// Everything that defines a game mode: board, hand, pieces, clearing patterns, scoring,
    /// and which rules to use (by id, created through <see cref="RuleFactory"/>).
    /// Plain serializable data, so it can be edited in Unity or loaded from remote config.
    /// The seed is not part of config: config is the mode, the seed is one run of it.
    /// </summary>
    [Serializable]
    public sealed class SimConfig
    {
        public int BoardWidth = 8;

        public int BoardHeight = 8;

        public int HandSize = 3;

        public string DealerId = RuleIds.WeightedRandomDealer;

        public string ScoringRuleId = RuleIds.DefaultScoring;

        /// <summary>Checked in order after every turn; the first one that resolves the game wins.</summary>
        public List<string> ResolutionRuleIds = new List<string> { RuleIds.NoMovesLeft };

        public List<PieceDefinition> Pieces = new List<PieceDefinition>();

        public ClearingConfig Clearing = new ClearingConfig();

        public ScoringConfig Scoring = new ScoringConfig();

        /// <summary>A deep copy: edits to either one don't affect the other.</summary>
        internal SimConfig Clone()
        {
            var copy = (SimConfig)MemberwiseClone();

            if (ResolutionRuleIds != null)
                copy.ResolutionRuleIds = new List<string>(ResolutionRuleIds);

            if (Pieces != null)
                copy.Pieces = Pieces.ConvertAll(piece => piece?.Clone());

            copy.Clearing = Clearing?.Clone();
            copy.Scoring = Scoring?.Clone();

            return copy;
        }

        /// <summary>Throws <see cref="InvalidOperationException"/> if the config can't run a game.</summary>
        public void Validate()
        {
            if (BoardWidth <= 0 || BoardHeight <= 0)
                throw new InvalidOperationException($"Board size must be positive, got {BoardWidth}x{BoardHeight}.");

            if (HandSize <= 0)
                throw new InvalidOperationException($"Hand size must be positive, got {HandSize}.");

            if (Pieces == null || Pieces.Count == 0)
                throw new InvalidOperationException("Config has no pieces.");

            if (Clearing == null)
                throw new InvalidOperationException("Config has no clearing settings.");

            if (Scoring == null)
                throw new InvalidOperationException("Config has no scoring settings.");

            if (ResolutionRuleIds == null)
                throw new InvalidOperationException("Config has no resolution rule list.");

            int totalWeight = 0;

            foreach (PieceDefinition piece in Pieces)
            {
                if (piece.Weight < 0)
                    throw new InvalidOperationException($"Piece '{piece.Id}' has a negative weight.");

                totalWeight += piece.Weight;
            }

            if (totalWeight == 0)
                throw new InvalidOperationException("All pieces have zero weight.");
        }
    }
}
