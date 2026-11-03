namespace Squarely.Simulation
{
    public sealed class ComboChangedEvent : ISimEvent
    {
        public ComboChangedEvent(int previous, int current)
        {
            Previous = previous;
            Current = current;
        }

        public int Previous { get; }

        public int Current { get; }
    }
}
