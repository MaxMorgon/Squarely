using System.Collections.Generic;

namespace Squarely.Simulation
{
    /// <summary>
    /// Computes points and the new combo for one placement.
    /// </summary>
    public interface IScoringRule
    {
        ScoreResult Score(int cellsPlaced, IReadOnlyList<PatternMatch> matches, int currentCombo);
    }
}
