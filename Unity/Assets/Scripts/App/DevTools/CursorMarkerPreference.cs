using System;

namespace Squarely.App
{
    /// <summary>
    /// The "Show cursor" dev tools setting, saved in a store so it stays between app runs. Off until the player turns it on.
    /// The cursor marker shows only while this and the dev tools are both on.
    /// </summary>
    public sealed class CursorMarkerPreference
    {
        /// <summary>The key the setting is saved under, as 1 (on) or 0 (off).</summary>
        public const string StoreKey = "CursorMarkerOn";

        private readonly IKeyValueStore _store;

        /// <summary>The saved setting.</summary>
        public bool IsOn => _store.GetInt(StoreKey, 0) != 0;

        /// <summary>The setting changed. The argument is the new value. Not raised when the value stays the same.</summary>
        public event Action<bool> ChangeEvent;

        public CursorMarkerPreference(IKeyValueStore store)
        {
            _store = store;
        }

        /// <summary>Sets the setting, saves it at once and raises <see cref="ChangeEvent"/> if the value changed.</summary>
        public void Set(bool isOn)
        {
            bool wasOn = IsOn;
            _store.SetInt(StoreKey, isOn ? 1 : 0);
            _store.Save();

            if (wasOn != isOn)
                ChangeEvent?.Invoke(isOn);
        }
    }
}
