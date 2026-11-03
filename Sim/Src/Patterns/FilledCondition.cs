namespace Squarely.Simulation
{
    /// <summary>
    /// Matches any non-empty cell.
    /// </summary>
    public sealed class FilledCondition : ICellCondition
    {
        public static readonly FilledCondition Instance = new FilledCondition();

        private FilledCondition()
        {
        }

        public bool Matches(Cell cell)
        {
            return !cell.IsEmpty;
        }
    }
}
