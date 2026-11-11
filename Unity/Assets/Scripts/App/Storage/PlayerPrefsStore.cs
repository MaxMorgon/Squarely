using UnityEngine;

namespace Squarely.App
{
    /// <summary>
    /// The device store backed by Unity's PlayerPrefs.
    /// </summary>
    public sealed class PlayerPrefsStore : IKeyValueStore
    {
        public bool HasKey(string key)
        {
            return PlayerPrefs.HasKey(key);
        }

        public int GetInt(string key, int defaultValue)
        {
            return PlayerPrefs.GetInt(key, defaultValue);
        }

        public void SetInt(string key, int value)
        {
            PlayerPrefs.SetInt(key, value);
        }

        public string GetString(string key, string defaultValue)
        {
            return PlayerPrefs.GetString(key, defaultValue);
        }

        public void SetString(string key, string value)
        {
            PlayerPrefs.SetString(key, value);
        }

        public void Save()
        {
            PlayerPrefs.Save();
        }
    }
}
