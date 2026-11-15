using UnityEngine;

namespace Squarely.Presentation
{
    /// <summary>
    /// Tunables for replay watching: the speed choices and the pace of auto-play.
    /// </summary>
    [CreateAssetMenu(menuName = "Squarely/Replay Feel", fileName = "ReplayFeel")]
    public sealed class ReplayFeel : ScriptableObject
    {
        [Tooltip("The playback speeds the speed dropdown offers, in order.")]
        [SerializeField] private float[] _speedPresets = { 4f, 2f, 1f, 0.5f };

        [Tooltip("Seconds auto-play waits, at speed 1, between the end of a move's playback and the next move.")]
        [SerializeField] private float _stepDelay = 0.4f;

        /// <summary>The playback speeds the speed dropdown offers, in order.</summary>
        public float[] SpeedPresets => _speedPresets;

        /// <summary>Seconds auto-play waits between a move's playback and the next move, at speed 1.</summary>
        public float StepDelay => _stepDelay;
    }
}
