namespace Squarely.Simulation
{
    /// <summary>
    /// A player or system action. Commands are plain data; a seed plus a list of commands is a full replay.
    /// Run them through <see cref="Sim.Execute"/>.
    /// </summary>
    public interface ISimCommand
    {
        CommandResult Execute(Sim sim);
    }
}
