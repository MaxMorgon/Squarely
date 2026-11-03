namespace Squarely.Simulation
{
    /// <summary>
    /// A test one cell must pass for a board pattern to match.
    /// Later conditions can also check board layers (e.g. "on fire").
    /// </summary>
    public interface ICellCondition
    {
        bool Matches(Cell cell);
    }
}
