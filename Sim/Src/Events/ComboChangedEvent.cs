namespace Squarely.Simulation
{
    public sealed class ComboChangedEvent : ISimEvent
    {
        public int Previous { get; }

        public int Current { get; }

        public ComboChangedEvent(int previous, int current)
        {
            Previous = previous;
            Current = current;
        }
    }
}
