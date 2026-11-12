using Squarely.Simulation;

namespace Squarely.App
{
    /// <summary>Chooses the one jingle that plays when a game ends.</summary>
    public static class GameOverJingle
    {
        public enum Kind
        {
            Lost,
            Won,
            NewRecord
        }

        /// <summary>A new record plays the record jingle instead of the outcome jingle.</summary>
        public static Kind Choose(GameOutcome outcome, bool isNewRecord)
        {
            if (isNewRecord)
                return Kind.NewRecord;

            return outcome == GameOutcome.Won ? Kind.Won : Kind.Lost;
        }
    }
}
