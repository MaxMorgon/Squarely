using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Squarely.Editor
{
    /// <summary>
    /// Builds the player. Two configs: Store has no dev tools; Studio is a normal (non-development) build
    /// with the SQUARELY_DEV_TOOLS define, so dev tools are compiled in and the "Dev tools" setting starts on.
    /// Output goes to Builds/(Target)-(Config)/ at the repo root.
    /// </summary>
    public static class BuildTool
    {
        public enum Config
        {
            Store,
            Studio
        }

        public enum Target
        {
            Android,
            WebGL
        }

        private const string DevToolsDefine = "SQUARELY_DEV_TOOLS";

        #region Menu

        [MenuItem("Squarely/Build/Android Store")]
        private static void BuildAndroidStore()
        {
            Build(Config.Store, Target.Android);
        }

        [MenuItem("Squarely/Build/Android Studio")]
        private static void BuildAndroidStudio()
        {
            Build(Config.Studio, Target.Android);
        }

        [MenuItem("Squarely/Build/WebGL Store")]
        private static void BuildWebGLStore()
        {
            Build(Config.Store, Target.WebGL);
        }

        [MenuItem("Squarely/Build/WebGL Studio")]
        private static void BuildWebGLStudio()
        {
            Build(Config.Studio, Target.WebGL);
        }

        #endregion

        /// <summary>
        /// Builds the player for the config and target. Throws if the build fails (for example when the platform module is missing).
        /// Returns the output folder.
        /// </summary>
        public static string Build(Config config, Target target)
        {
            string outputFolder = GetOutputFolder(config, target);
            Directory.CreateDirectory(outputFolder);

            var options = new BuildPlayerOptions
            {
                scenes = GetEnabledScenes(),
                locationPathName = GetLocationPath(outputFolder, target),
                target = target == Target.Android ? BuildTarget.Android : BuildTarget.WebGL,
                options = BuildOptions.None,
                extraScriptingDefines = config == Config.Studio ? new[] { DevToolsDefine } : Array.Empty<string>()
            };

            BuildReport report = BuildPipeline.BuildPlayer(options);

            if (report.summary.result != BuildResult.Succeeded)
                throw new InvalidOperationException($"Build {target}-{config} failed: {report.summary.result}, {report.summary.totalErrors} errors.");

            Debug.Log($"Build {target}-{config} succeeded: {outputFolder}");
            return outputFolder;
        }

        /// <summary>The build output folder: Builds/(Target)-(Config)/ at the repo root.</summary>
        public static string GetOutputFolder(Config config, Target target)
        {
            // Application.dataPath is (repo)/Unity/Assets.
            string repoRoot = Path.GetFullPath(Path.Combine(Application.dataPath, "..", ".."));
            return Path.Combine(repoRoot, "Builds", $"{target}-{config}");
        }

        private static string GetLocationPath(string outputFolder, Target target)
        {
            return target == Target.Android ? Path.Combine(outputFolder, "Squarely.apk") : outputFolder;
        }

        private static string[] GetEnabledScenes()
        {
            var scenes = new List<string>();

            foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
            {
                if (scene.enabled)
                    scenes.Add(scene.path);
            }

            return scenes.ToArray();
        }
    }
}
