namespace Squarely.Simulation
{
    public sealed class GameResolvedEvent : ISimEvent
    {
        public GameResolvedEvent(GameResolution resolution, int finalScore)
        {
            Resolution = resolution;
            FinalScore = finalScore;
        }

        public GameResolution Resolution { get; }

        public int FinalScore { get; }
    }
}
