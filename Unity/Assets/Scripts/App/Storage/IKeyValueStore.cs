namespace Squarely.App
{
    /// <summary>
    /// Values saved on the device by key, kept between app runs.
    /// Ints and strings share one key space: a key holds one value.
    /// </summary>
    public interface IKeyValueStore
    {
        bool HasKey(string key);

        /// <summary>The int saved under the key, or the default when there is none.</summary>
        int GetInt(string key, int defaultValue);

        void SetInt(string key, int value);

        /// <summary>The string saved under the key, or the default when there is none.</summary>
        string GetString(string key, string defaultValue);

        void SetString(string key, string value);

        /// <summary>Writes the values set so far to the device, so they survive the app being killed.</summary>
        void Save();
    }
}
