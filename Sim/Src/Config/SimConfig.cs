using System;
using System.Collections.Generic;

namespace Squarely.Simulation
{
    /// <summary>
    /// All tunable values for a sim session. Plain data, so it can be loaded from JSON or remote config later.
    /// </summary>
    public sealed class SimConfig
    {
        public int BoardWidth { get; set; } = 8;

        public int BoardHeight { get; set; } = 8;

        public int HandSize { get; set; } = 3;

        public List<PieceDefinition> Pieces { get; set; } = new List<PieceDefinition>();

        public ScoringConfig Scoring { get; set; } = new ScoringConfig();

        public void Validate()
        {
            if (BoardWidth <= 0 || BoardHeight <= 0)
                throw new InvalidOperationException($"Board size must be positive, got {BoardWidth}x{BoardHeight}.");

            if (HandSize <= 0)
                throw new InvalidOperationException($"Hand size must be positive, got {HandSize}.");

            if (Pieces == null || Pieces.Count == 0)
                throw new InvalidOperationException("Config has no pieces.");

            if (Scoring == null)
                throw new InvalidOperationException("Config has no scoring settings.");

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
