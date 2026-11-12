using UnityEngine;

namespace Squarely.Presentation
{
    /// <summary>
    /// Tunables for how a dragged piece feels. Edits apply from the next drag.
    /// </summary>
    [CreateAssetMenu(menuName = "Squarely/Drag Feel", fileName = "DragFeel")]
    public sealed class DragFeel : ScriptableObject
    {
        [Header("Lift")]
        [Tooltip("How far above the pointer the dragged piece floats, in world units.")]
        [SerializeField, Min(0f)] private float _liftOffset = 2f;

        [Header("Swing")]
        [Tooltip("Largest tilt of the dragged piece, in degrees.")]
        [SerializeField, Range(0f, 45f)] private float _maxAngle = 12f;

        [Tooltip("Target tilt in degrees for a pointer speed of one world unit per second.")]
        [SerializeField, Min(0f)] private float _degreesPerSpeed = 1f;

        [Tooltip("Spring frequency in hertz. Higher is snappier.")]
        [SerializeField, Min(0.1f)] private float _springFrequency = 3f;

        [Tooltip("0 never settles, 1 settles without overshoot. About 0.65 gives a small overshoot.")]
        [SerializeField, Range(0f, 2f)] private float _dampingRatio = 0.65f;

        [Tooltip("Time constant in seconds for smoothing the pointer speed. 0 turns smoothing off.")]
        [SerializeField, Min(0f)] private float _velocitySmoothing = 0.05f;

        /// <summary>How far above the pointer the dragged piece floats, in world units.</summary>
        public float LiftOffset => _liftOffset;

        /// <summary>Creates a swing with the current values.</summary>
        public DragSwing CreateSwing()
        {
            return new DragSwing(_maxAngle, _degreesPerSpeed, _springFrequency, _dampingRatio, _velocitySmoothing);
        }
    }
}
