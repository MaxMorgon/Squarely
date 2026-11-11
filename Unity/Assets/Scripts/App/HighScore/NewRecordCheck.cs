namespace Squarely.App
{
    /// <summary>
    /// Tells when a game's score first goes above the best score, so the game celebrates a new record once.
    /// </summary>
    public sealed class NewRecordCheck
    {
        private readonly int? _best;
        private bool _isReached;

        /// <param name="best">The best score when the game starts, or null if there is none yet. Null and 0 both mean no best score.</param>
        public NewRecordCheck(int? best)
        {
            _best = best;
        }

        /// <summary>
        /// Returns true the first time the score is above the best score, and false after that.
        /// Always false when there is no best score (null or 0).
        /// </summary>
        public bool Check(int score)
        {
            if (_isReached || !_best.HasValue || _best.Value <= 0 || score <= _best.Value)
                return false;

            _isReached = true;
            return true;
        }
    }
}
