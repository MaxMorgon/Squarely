namespace Squarely.Simulation
{
    public sealed class ScoreChangedEvent : ISimEvent
    {
        public int Delta { get; }

        public int Total { get; }

        public ScoreChangedEvent(int delta, int total)
        {
            Delta = delta;
            Total = total;
        }
    }
}
