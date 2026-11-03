namespace Squarely.Simulation
{
    /// <summary>
    /// Tunable scoring values. Integers only, to keep scoring deterministic.
    /// </summary>
    public sealed class ScoringConfig
    {
        /// <summary>Points for each cell of a placed piece.</summary>
        public int PointsPerCellPlaced { get; set; } = 1;

        /// <summary>Points for each cleared row or column.</summary>
        public int PointsPerLine { get; set; } = 10;

        /// <summary>Extra points for each line beyond the first in a single placement.</summary>
        public int MultiLineBonusPerExtraLine { get; set; } = 10;

        /// <summary>
        /// Percent added to line points per combo step beyond the first.
        /// Combo 1 = 100%, combo 2 = 100% + this, and so on.
        /// </summary>
        public int ComboBonusPercent { get; set; } = 50;
    }
}
