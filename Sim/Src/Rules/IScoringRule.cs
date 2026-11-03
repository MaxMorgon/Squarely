namespace Squarely.Simulation
{
    /// <summary>
    /// Computes points and the new combo for one placement.
    /// </summary>
    public interface IScoringRule
    {
        ScoreResult Score(int cellsPlaced, int linesCleared, int currentCombo);
    }

    public readonly struct ScoreResult
    {
        public readonly int Points;
        public readonly int NewCombo;

        public ScoreResult(int points, int newCombo)
        {
            Points = points;
            NewCombo = newCombo;
        }
    }
}
