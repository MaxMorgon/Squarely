using System.Collections.Generic;

namespace Squarely.App.Tests
{
    /// <summary>
    /// In-memory store for tests. Counts its Save calls.
    /// </summary>
    internal sealed class FakeKeyValueStore : IKeyValueStore
    {
        private readonly Dictionary<string, int> _ints = new Dictionary<string, int>();
        private readonly Dictionary<string, string> _strings = new Dictionary<string, string>();

        public int SaveCount { get; private set; }

        public bool HasKey(string key)
        {
            return _ints.ContainsKey(key) || _strings.ContainsKey(key);
        }

        public int GetInt(string key, int defaultValue)
        {
            return _ints.TryGetValue(key, out int value) ? value : defaultValue;
        }

        public void SetInt(string key, int value)
        {
            _strings.Remove(key);
            _ints[key] = value;
        }

        public string GetString(string key, string defaultValue)
        {
            return _strings.TryGetValue(key, out string value) ? value : defaultValue;
        }

        public void SetString(string key, string value)
        {
            _ints.Remove(key);
            _strings[key] = value;
        }

        public void Save()
        {
            SaveCount++;
        }
    }
}
