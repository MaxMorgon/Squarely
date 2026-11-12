using System;
using Unity.Pipeline.Commands;

namespace Squarely.Editor
{
    /// <summary>
    /// CLI command that builds the player, see <see cref="BuildTool"/>.
    /// Run from the Unity project folder, for example: build player studio webgl
    /// </summary>
    public static class BuildCliCommands
    {
        /// <param name="config">store or studio.</param>
        /// <param name="target">android or webgl.</param>
        [CliCommand("build", "player", "Build the player into Builds/(Target)-(Config)/: config store|studio, target android|webgl")]
        public static string BuildPlayer(string config, string target)
        {
            var parsedConfig = (BuildTool.Config)Enum.Parse(typeof(BuildTool.Config), config, true);
            var parsedTarget = (BuildTool.Target)Enum.Parse(typeof(BuildTool.Target), target, true);
            return "Built to " + BuildTool.Build(parsedConfig, parsedTarget);
        }
    }
}
