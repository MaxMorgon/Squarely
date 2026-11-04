namespace Squarely.Simulation
{
    /// <summary>
    /// Decides whether the game is resolved (won or lost). Checked after every turn and at start.
    /// Must hold no mutable state: one instance is shared by a sim and its preview clones.
    /// </summary>
    public interface IResolutionRule
    {
        /// <summary>Returns the resolution, or null if the game goes on.</summary>
        GameResolution Check(Sim sim);
    }
}
