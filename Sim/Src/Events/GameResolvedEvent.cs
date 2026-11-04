namespace Squarely.Simulation
{
    public sealed class GameResolvedEvent : ISimEvent
    {
        public GameResolution Resolution { get; }

        public int FinalScore { get; }

        public GameResolvedEvent(GameResolution resolution, int finalScore)
        {
            Resolution = resolution;
            FinalScore = finalScore;
        }
    }
}
