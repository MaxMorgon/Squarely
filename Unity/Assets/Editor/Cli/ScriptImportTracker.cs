using UnityEditor;

namespace Squarely.Editor
{
    /// <summary>
    /// Records whether an import touched any script or assembly definition, i.e. whether a recompile will follow.
    /// Runs synchronously during AssetDatabase.Refresh, so callers can check it right after refreshing.
    /// </summary>
    internal sealed class ScriptImportTracker : AssetPostprocessor
    {
        public static bool ScriptsChanged { get; private set; }

        public static void Reset()
        {
            ScriptsChanged = false;
        }

        private static void OnPostprocessAllAssets(
            string[] importedAssets,
            string[] deletedAssets,
            string[] movedAssets,
            string[] movedFromAssetPaths)
        {
            if (ContainsScript(importedAssets) || ContainsScript(deletedAssets) || ContainsScript(movedAssets))
                ScriptsChanged = true;
        }

        private static bool ContainsScript(string[] paths)
        {
            foreach (string path in paths)
            {
                if (path.EndsWith(".cs") || path.EndsWith(".asmdef") || path.EndsWith(".asmref"))
                    return true;
            }

            return false;
        }
    }
}
