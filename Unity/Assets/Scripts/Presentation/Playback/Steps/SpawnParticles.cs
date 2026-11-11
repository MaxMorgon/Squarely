using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Squarely.Presentation
{
    /// <summary>Spawns an effect prefab at the target's position and lets it play out. Snapping leaves a playing effect alone.</summary>
    [Serializable]
    public sealed class SpawnParticles : CueStep
    {
        [Tooltip("The effect to spawn, e.g. a particle system. Nothing spawns if empty.")]
        [SerializeField] private GameObject _prefab;

        [Tooltip("Seconds until the spawned effect is destroyed.")]
        [SerializeField] private float _lifetime = 2f;

        public override Awaitable Play(CueTarget target, PlaybackContext context)
        {
            if (_prefab != null)
            {
                GameObject effect = Object.Instantiate(_prefab, target.Position, Quaternion.identity, context.Views.EffectsRoot);
                Object.Destroy(effect, _lifetime);
            }

            return Completed();
        }

        public override void Snap(CueTarget target, PlaybackContext context)
        {
        }
    }
}
