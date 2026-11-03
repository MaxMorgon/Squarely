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
        /// Script changes trigger a recompile afterwards; poll "console status" for completion.
        /// </summary>
        [CliCommand("asset", "refresh", "Refresh the asset database: import new/changed files and generate .meta files")]
        public static string RefreshAssets()
        {
            AssetDatabase.Refresh();

            if (EditorApplication.isCompiling)
                return "Assets refreshed. Scripts are recompiling.";

            return "Assets refreshed.";
        }
    }
}
