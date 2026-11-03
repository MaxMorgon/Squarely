using System;
using System.Collections.Generic;

namespace Squarely.Simulation
{
    /// <summary>
    /// Which board patterns clear cells when completed. Full rows and columns are built in;
    /// custom patterns are added on top.
    /// </summary>
    [Serializable]
    public sealed class ClearingConfig
    {
        public bool ClearRows = true;

        public bool ClearColumns = true;

        /// <summary>Points for each completed row or column.</summary>
        public int LinePoints = 10;

        public List<BoardPatternDefinition> Patterns = new List<BoardPatternDefinition>();

        /// <summary>
        /// Builds the runtime pattern list: rows (bottom to top), columns (left to right), then custom patterns.
        /// </summary>
        public List<BoardPattern> BuildPatterns(int boardWidth, int boardHeight)
        {
            var patterns = new List<BoardPattern>();

            if (ClearRows)
            {
                for (int y = 0; y < boardHeight; y++)
                    patterns.Add(BoardPattern.Row(y, boardWidth, LinePoints));
            }

            if (ClearColumns)
            {
                for (int x = 0; x < boardWidth; x++)
                    patterns.Add(BoardPattern.Column(x, boardHeight, LinePoints));
            }

            if (Patterns != null)
            {
                foreach (BoardPatternDefinition definition in Patterns)
                    patterns.Add(definition.ToPattern());
            }

            return patterns;
        }
    }
}
