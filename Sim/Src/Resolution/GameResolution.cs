namespace Squarely.Simulation
{
    /// <summary>
    /// How a game was resolved: the outcome and the id of the reason (e.g. "no_moves_left").
    /// </summary>
    public sealed class GameResolution
    {
        public GameResolution(GameOutcome outcome, string reasonId)
        {
            Outcome = outcome;
            ReasonId = reasonId;
        }

        public GameOutcome Outcome { get; }

        public string ReasonId { get; }

        public override string ToString()
        {
            return $"{Outcome} ({ReasonId})";
        }
    }
}
