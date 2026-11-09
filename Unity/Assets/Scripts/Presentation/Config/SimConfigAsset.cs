using Squarely.Simulation;
using UnityEngine;

namespace Squarely.Presentation
{
    /// <summary>
    /// A game mode's <see cref="SimConfig"/> as an editable asset. New assets start with the default config.
    /// </summary>
    [CreateAssetMenu(menuName = "Squarely/Sim Config", fileName = "SimConfig")]
    public sealed class SimConfigAsset : ScriptableObject
    {
        [SerializeField] private SimConfig _config = DefaultSimConfig.Create();

        /// <summary>The asset's own config, not a copy: edits, also in play mode, reach the sims that use it.</summary>
        public SimConfig Config => _config;
    }
}
