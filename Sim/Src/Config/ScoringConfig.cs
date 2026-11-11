using System;

namespace Squarely.Simulation
{
    /// <summary>
    /// Tunable values for the default scoring rule. Integers only, to keep scoring deterministic.
    /// Points per completed pattern come from the pattern itself (see <see cref="ClearingConfig"/>).
    /// </summary>
    [Serializable]
    public sealed class ScoringConfig
    {
        /// <summary>Points for each cell of a placed piece.</summary>
        public int PointsPerCellPlaced = 1;

        /// <summary>Extra points for each pattern beyond the first completed by a single placement.</summary>
        public int MultiMatchBonusPerExtra = 10;

        /// <summary>
        /// Percent added to pattern points per combo step beyond the first.
        /// Combo 1 = 100%, combo 2 = 100% + this, and so on.
        /// </summary>
        public int ComboBonusPercent = 50;

        /// <summary>An independent copy: edits to either one don't affect the other.</summary>
        internal ScoringConfig Clone()
        {
            return (ScoringConfig)MemberwiseClone();
        }
    }
}
