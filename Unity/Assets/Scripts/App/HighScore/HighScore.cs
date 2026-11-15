namespace Squarely.App
{
    /// <summary>
    /// The best score of any finished game on this device, saved in a store.
    /// One high score for every game mode.
    /// </summary>
    public sealed class HighScore
    {
        /// <summary>The key the high score is saved under.</summary>
        public const string StoreKey = "HighScore";

        private readonly IKeyValueStore _store;

        /// <summary>The best score so far, or null before the first finished game.</summary>
        public int? Best
        {
            get
            {
                if (!_store.HasKey(StoreKey))
                    return null;

                return _store.GetInt(StoreKey, 0);
            }
        }

        public HighScore(IKeyValueStore store)
        {
            _store = store;
        }

        /// <summary>
        /// Records the score of a finished game. Returns true if it is a new high score:
        /// the first finished game, or a score above the best so far. A new high score is saved at once.
        /// </summary>
        public bool Record(int score)
        {
            int? best = Best;

            if (best.HasValue && score <= best.Value)
                return false;

            _store.SetInt(StoreKey, score);
            _store.Save();
            return true;
        }

        /// <summary>Forgets the best score and saves at once. <see cref="Best"/> is null again.</summary>
        public void Reset()
        {
            _store.DeleteKey(StoreKey);
            _store.Save();
        }
    }
}
