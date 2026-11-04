using System.Collections.Generic;

namespace Squarely.Simulation
{
    /// <summary>
    /// Computes points and the new combo for one placement.
    /// Must hold no mutable state: one instance is shared by a sim and its preview clones.
    /// </summary>
    public interface IScoringRule
    {
        ScoreResult Score(int cellsPlaced, IReadOnlyList<PatternMatch> matches, int currentCombo);
    }
}
