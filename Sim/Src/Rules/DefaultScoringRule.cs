using System;

namespace Squarely.Simulation
{
    /// <summary>
    /// Points for placed cells, plus line points with a multi-line bonus, multiplied by the combo.
    /// A placement that clears lines increases the combo; one that clears nothing resets it to 0.
    /// </summary>
    public sealed class DefaultScoringRule : IScoringRule
    {
        private readonly ScoringConfig _config;

        public DefaultScoringRule(ScoringConfig config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        public ScoreResult Score(int cellsPlaced, int linesCleared, int currentCombo)
        {
            int placementPoints = cellsPlaced * _config.PointsPerCellPlaced;

            if (linesCleared == 0)
                return new ScoreResult(placementPoints, 0);

            int newCombo = currentCombo + 1;
            int linePoints = linesCleared * _config.PointsPerLine
                + (linesCleared - 1) * _config.MultiLineBonusPerExtraLine;
            int comboPercent = 100 + (newCombo - 1) * _config.ComboBonusPercent;
            int comboLinePoints = linePoints * comboPercent / 100;

            return new ScoreResult(placementPoints + comboLinePoints, newCombo);
        }
    }
}
