using System.Text;

namespace Squarely.Simulation
{
    /// <summary>
    /// A stable 64-bit hash of everything in a <see cref="SimConfig"/> that changes play (FNV-1a over a fixed byte layout).
    /// The same config gives the same value on every machine and runtime.
    /// When you add a config field, add it here too: a guard test fails otherwise.
    /// </summary>
    public static class ConfigFingerprint
    {
        private const ulong OffsetBasis = 14695981039346656037UL;
        private const ulong Prime = 1099511628211UL;

        public static ulong Compute(SimConfig config)
        {
            var hash = new Hasher();

            hash.Add(config.BoardWidth);
            hash.Add(config.BoardHeight);
            hash.Add(config.HandSize);
            hash.Add(config.DealerId);
            hash.Add(config.ScoringRuleId);

            hash.Add(config.ResolutionRuleIds.Count);
            foreach (string id in config.ResolutionRuleIds)
                hash.Add(id);

            hash.Add(config.Pieces.Count);
            foreach (PieceDefinition piece in config.Pieces)
            {
                hash.Add(piece.Id);
                hash.Add(piece.Pattern);
                hash.Add(piece.Weight);
                hash.Add(piece.CellTypeId);
            }

            hash.Add(config.Dealing.GuaranteeHelpfulPiece);

            hash.Add(config.Clearing.ClearRows);
            hash.Add(config.Clearing.ClearColumns);
            hash.Add(config.Clearing.LinePoints);
            hash.Add(config.Clearing.Patterns.Count);
            foreach (BoardPatternDefinition pattern in config.Clearing.Patterns)
            {
                hash.Add(pattern.Id);
                hash.Add(pattern.Pattern);
                hash.Add(pattern.Points);
                hash.Add(pattern.IsAnchored);
                hash.Add(pattern.AnchorX);
                hash.Add(pattern.AnchorY);
            }

            hash.Add(config.Scoring.PointsPerCellPlaced);
            hash.Add(config.Scoring.MultiMatchBonusPerExtra);
            hash.Add(config.Scoring.ComboBonusPercent);

            return hash.Value;
        }

        private sealed class Hasher
        {
            public ulong Value { get; private set; } = OffsetBasis;

            public void Add(int value)
            {
                AddByte((byte)value);
                AddByte((byte)(value >> 8));
                AddByte((byte)(value >> 16));
                AddByte((byte)(value >> 24));
            }

            public void Add(bool value)
            {
                AddByte(value ? (byte)1 : (byte)0);
            }

            /// <summary>Length-prefixed UTF-8, so neighbouring strings can't blur together. Null differs from empty.</summary>
            public void Add(string value)
            {
                if (value == null)
                {
                    Add(-1);
                    return;
                }

                byte[] bytes = Encoding.UTF8.GetBytes(value);
                Add(bytes.Length);

                foreach (byte b in bytes)
                    AddByte(b);
            }

            private void AddByte(byte b)
            {
                Value ^= b;
                Value *= Prime;
            }
        }
    }
}
