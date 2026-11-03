namespace Squarely.Simulation
{
    public sealed class ScoreChangedEvent : ISimEvent
    {
        public ScoreChangedEvent(int delta, int total)
        {
            Delta = delta;
            Total = total;
        }

        public int Delta { get; }

        public int Total { get; }
    }
}
