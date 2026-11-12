namespace Squarely.Simulation
{
    /// <summary>
    /// Ends the game at once as a loss, with the same resolution as when no piece fits.
    /// Fails if the game is already resolved.
    /// </summary>
    public sealed class EndGameCommand : ISimCommand
    {
        public CommandResult Execute(Sim sim)
        {
            return sim.EndGame();
        }

        public override string ToString()
        {
            return "End game";
        }
    }
}
