using System.Collections.Generic;

namespace Squarely.App
{
    /// <summary>
    /// A store that keeps its values in memory only: nothing reaches the device, and the values are gone with the store.
    /// </summary>
    public sealed class MemoryKeyValueStore : IKeyValueStore
    {
        private readonly Dictionary<string, int> _ints = new Dictionary<string, int>();
        private readonly Dictionary<string, string> _strings = new Dictionary<string, string>();

        /// <summary>How many times <see cref="Save"/> was called.</summary>
        internal int SaveCount { get; private set; }

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

        /// <summary>Writes nothing to the device: the values stay in memory.</summary>
        public void Save()
        {
            SaveCount++;
        }
    }
}
