namespace Squarely.App
{
    /// <summary>
    /// What the player has earned on this device: today the high score only.
    /// Reset clears all of it, and keeps the settings, the replay history and the dev tools values.
    /// Add new meta progress here, so Reset clears it too.
    /// </summary>
    public sealed class PlayerProgress
    {
        private readonly HighScore _highScore;

        public PlayerProgress(HighScore highScore)
        {
            _highScore = highScore;
        }

        /// <summary>Clears the player's progress and saves at once.</summary>
        public void Reset()
        {
            _highScore.Reset();
        }
    }
}
