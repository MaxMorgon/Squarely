namespace Squarely.Simulation
{
    /// <summary>
    /// Output of an <see cref="IScoringRule"/>: points gained this placement and the combo afterwards.
    /// </summary>
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
