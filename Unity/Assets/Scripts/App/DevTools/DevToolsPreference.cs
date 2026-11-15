using System;

namespace Squarely.App
{
    /// <summary>
    /// The player's "Dev tools" setting, saved in a store so it stays between app runs. On until the player turns it off.
    /// It only matters in builds with dev tools compiled in (the editor, and builds with the SQUARELY_DEV_TOOLS define).
    /// </summary>
    public sealed class DevToolsPreference
    {
        /// <summary>The key the setting is saved under, as 1 (on) or 0 (off).</summary>
        public const string StoreKey = "DevToolsOn";

        /// <summary>Whether this build has the dev tools code. False in store builds.</summary>
#if UNITY_EDITOR || SQUARELY_DEV_TOOLS
        public const bool IsCompiledIn = true;
#else
        public const bool IsCompiledIn = false;
#endif

        private readonly IKeyValueStore _store;

        /// <summary>The saved setting. Does not check whether the build has dev tools, see <see cref="IsActive"/>.</summary>
        public bool IsOn => _store.GetInt(StoreKey, 1) != 0;

        /// <summary>Whether the dev tools run now: compiled in and turned on.</summary>
        public bool IsActive => IsCompiledIn && IsOn;

        /// <summary>The setting changed. The argument is the new value. Not raised when the value stays the same.</summary>
        public event Action<bool> ChangeEvent;

        public DevToolsPreference(IKeyValueStore store)
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
