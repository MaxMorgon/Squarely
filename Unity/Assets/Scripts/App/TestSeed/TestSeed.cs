#if UNITY_EDITOR || SQUARELY_DEV_TOOLS
using System;
using System.Globalization;

namespace Squarely.App
{
    /// <summary>
    /// The seed the test screen plays, saved in a store so it stays the same between app runs.
    /// Seeds are shown and saved as decimal text, since they don't fit in the store's int.
    /// </summary>
    public sealed class TestSeed
    {
        /// <summary>The key the seed is saved under.</summary>
        public const string StoreKey = "TestSeed";

        private readonly IKeyValueStore _store;
        private readonly SeedSource _seedSource;

        /// <param name="seedSource">Picks new seeds.</param>
        public TestSeed(IKeyValueStore store, SeedSource seedSource)
        {
            _store = store;
            _seedSource = seedSource;
        }

        #region Seeds

        /// <summary>The saved seed. When none is saved, or the saved text is not a seed, picks a new seed and saves it.</summary>
        public ulong LoadOrCreate()
        {
            string text = _store.GetString(StoreKey, null);

            if (TryParse(text, out ulong seed))
                return seed;

            return CreateNew();
        }

        /// <summary>Picks a new seed from the seed source and saves it.</summary>
        public ulong CreateNew()
        {
            ulong seed = _seedSource.Next();
            Save(seed);
            return seed;
        }

        /// <summary>Saves the seed, replacing the saved one.</summary>
        public void Save(ulong seed)
        {
            _store.SetString(StoreKey, Format(seed));
            _store.Save();
        }

        #endregion

        #region Text

        /// <summary>
        /// Reads a seed typed by the player: decimal digits only, surrounding spaces ignored, 0 to ulong.MaxValue.
        /// Returns false for anything else, including empty or null text.
        /// </summary>
        public static bool TryParse(string text, out ulong seed)
        {
            seed = 0;

            if (text == null)
                return false;

            return ulong.TryParse(text.Trim(), NumberStyles.None, CultureInfo.InvariantCulture, out seed);
        }

        /// <summary>The seed as decimal text, the form <see cref="TryParse"/> reads.</summary>
        public static string Format(ulong seed)
        {
            return seed.ToString(CultureInfo.InvariantCulture);
        }

        #endregion
    }
}
#endif
