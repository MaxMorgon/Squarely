using Unity.Pipeline.Commands;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace Squarely.Editor
{
    /// <summary>
    /// Project-specific Unity CLI commands for scenes.
    /// Run from the Unity project folder: unity command scene reopen-main
    /// </summary>
    public static class SceneCliCommands
    {
        private const string MainScenePath = "Assets/Scenes/Main.unity";

        /// <summary>
        /// Opens the main scene again from disk, so that the editor shows the version on the current branch.
        /// Does nothing in play mode or when an open scene has unsaved changes.
        /// </summary>
        [CliCommand("scene", "reopen-main", "Reopen Main.unity from disk (refuses in play mode or with unsaved scene changes)")]
        public static string ReopenMain()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                return "Not reopened: the editor is in play mode.";

            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);

                if (scene.isDirty)
                    return $"Not reopened: scene '{scene.name}' has unsaved changes.";
            }

            EditorSceneManager.OpenScene(MainScenePath, OpenSceneMode.Single);
            return $"Reopened {MainScenePath}.";
        }
    }
}
