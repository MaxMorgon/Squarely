namespace Squarely.Simulation
{
    /// <summary>
    /// Matches only empty cells.
    /// </summary>
    public sealed class EmptyCondition : ICellCondition
    {
        public static readonly EmptyCondition Instance = new EmptyCondition();

        private EmptyCondition()
        {
        }

        public bool Matches(Cell cell)
        {
            return cell.IsEmpty;
        }
    }
}
