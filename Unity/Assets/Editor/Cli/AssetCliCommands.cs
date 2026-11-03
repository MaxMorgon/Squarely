using Unity.Pipeline.Commands;
using UnityEditor;

namespace Squarely.Editor
{
    /// <summary>
    /// Project-specific Unity CLI commands for asset maintenance.
    /// Run from the Unity project folder: unity command asset refresh
    /// </summary>
    public static class AssetCliCommands
    {
        /// <summary>
        /// Imports new and changed files (and generates their .meta files), same as focusing the editor window.
        /// If scripts changed, Unity recompiles after this returns; the result says so, and the caller
        /// should poll "script recompile_status" before sending further commands.
        /// </summary>
        [CliCommand("asset", "refresh", "Refresh the asset database: import new/changed files and generate .meta files")]
        public static string RefreshAssets()
        {
            ScriptImportTracker.Reset();
            AssetDatabase.Refresh();

            if (ScriptImportTracker.ScriptsChanged)
                return "Assets refreshed. Scripts changed: poll 'script recompile_status' until completed before further commands.";

            return "Assets refreshed. No script changes.";
        }
    }
}
