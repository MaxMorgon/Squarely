#if UNITY_EDITOR || SQUARELY_DEV_TOOLS
using Squarely.Presentation;

namespace Squarely.App
{
    /// <summary>
    /// Editor and dev-tools builds only (`SQUARELY_DEV_TOOLS`): shows the cursor marker while the dev tools and the "Show cursor" setting are both on.
    /// Follows both settings at once. Lives as long as the app.
    /// </summary>
    public sealed class CursorMarkerSwitch
    {
        private readonly CursorMarker _marker;
        private readonly DevToolsPreference _devTools;
        private readonly CursorMarkerPreference _preference;

        public CursorMarkerSwitch(CursorMarker marker, DevToolsPreference devTools, CursorMarkerPreference preference)
        {
            _marker = marker;
            _devTools = devTools;
            _preference = preference;

            _devTools.ChangeEvent += OnSettingChanged;
            _preference.ChangeEvent += OnSettingChanged;
            UpdateMarker();
        }

        private void OnSettingChanged(bool isOn)
        {
            UpdateMarker();
        }

        private void UpdateMarker()
        {
            if (_devTools.IsActive && _preference.IsOn)
                _marker.Show();
            else
                _marker.Hide();
        }
    }
}
#endif
