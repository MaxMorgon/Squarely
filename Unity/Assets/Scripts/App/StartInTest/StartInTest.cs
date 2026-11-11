namespace Squarely.App
{
    /// <summary>
    /// The test screen's "start in test screen" option, saved in a store so it stays between app runs.
    /// Off until it is set.
    /// </summary>
    public sealed class StartInTest
    {
        /// <summary>The key the option is saved under, as 1 (on) or 0 (off).</summary>
        public const string StoreKey = "StartInTest";

        private readonly IKeyValueStore _store;

        /// <summary>Whether the app opens the test screen at start instead of the menu.</summary>
        public bool IsOn => _store.GetInt(StoreKey, 0) != 0;

        public StartInTest(IKeyValueStore store)
        {
            _store = store;
        }

        /// <summary>Sets the option and saves it at once.</summary>
        public void Set(bool isOn)
        {
            _store.SetInt(StoreKey, isOn ? 1 : 0);
            _store.Save();
        }
    }
}
