using System.Globalization;

namespace Squarely.App
{
    /// <summary>
    /// The high score of games started from the test screen. It starts at the value the tester types
    /// and is never saved on the device.
    /// </summary>
    public static class TestHighScore
    {
        /// <summary>A high score with <paramref name="best"/> as its best score, kept in memory only.</summary>
        public static HighScore Create(int best)
        {
            var store = new MemoryKeyValueStore();
            store.SetInt(HighScore.StoreKey, best);
            return new HighScore(store);
        }

        /// <summary>
        /// Reads a high score typed by the tester: decimal digits only, surrounding spaces ignored, 0 to int.MaxValue.
        /// Returns false for anything else, including empty or null text.
        /// </summary>
        public static bool TryParse(string text, out int highScore)
        {
            highScore = 0;

            if (text == null)
                return false;

            return int.TryParse(text.Trim(), NumberStyles.None, CultureInfo.InvariantCulture, out highScore);
        }

        /// <summary>The high score as decimal text, the form <see cref="TryParse"/> reads.</summary>
        public static string Format(int highScore)
        {
            return highScore.ToString(CultureInfo.InvariantCulture);
        }
    }
}
