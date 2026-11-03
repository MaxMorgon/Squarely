using System;
using System.Collections.Generic;

namespace Squarely.Simulation
{
    /// <summary>
    /// Points for placed cells, plus each matched pattern's points with a multi-match bonus, multiplied by the combo.
    /// A placement that matches increases the combo; one that matches nothing resets it to 0.
    /// </summary>
    public sealed class DefaultScoringRule : IScoringRule
    {
        private readonly ScoringConfig _config;

        public DefaultScoringRule(ScoringConfig config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        public ScoreResult Score(int cellsPlaced, IReadOnlyList<PatternMatch> matches, int currentCombo)
        {
            int placementPoints = cellsPlaced * _config.PointsPerCellPlaced;

            if (matches.Count == 0)
                return new ScoreResult(placementPoints, 0);

            int patternPoints = 0;

            foreach (PatternMatch match in matches)
                patternPoints += match.Pattern.Points;

            patternPoints += (matches.Count - 1) * _config.MultiMatchBonusPerExtra;

            int newCombo = currentCombo + 1;
            int comboPercent = 100 + (newCombo - 1) * _config.ComboBonusPercent;
            int comboPatternPoints = patternPoints * comboPercent / 100;

            return new ScoreResult(placementPoints + comboPatternPoints, newCombo);
        }
    }
}
