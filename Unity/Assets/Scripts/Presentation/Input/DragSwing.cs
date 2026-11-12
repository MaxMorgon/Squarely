using System;

namespace Squarely.Presentation
{
    /// <summary>
    /// The tilt of a dragged piece. Follows the pointer's horizontal speed with a damped spring.
    /// Moving right gives a positive angle (the top leans left); moving left gives a negative angle.
    /// </summary>
    public sealed class DragSwing
    {
        private const float MaxStepSeconds = 1f / 240f;
        private const float MaxFrameSeconds = 0.1f;

        private readonly float _maxAngle;
        private readonly float _degreesPerSpeed;
        private readonly float _angularFrequency;
        private readonly float _dampingRatio;
        private readonly float _smoothingSeconds;

        private float _smoothedSpeed;
        private float _angleVelocity;

        /// <summary>The current tilt in degrees.</summary>
        public float Angle { get; private set; }

        /// <param name="maxAngle">Largest tilt in degrees, in both directions.</param>
        /// <param name="degreesPerSpeed">Target tilt in degrees for a pointer speed of one world unit per second.</param>
        /// <param name="frequency">Spring frequency in hertz.</param>
        /// <param name="dampingRatio">0 never settles; 1 settles without overshoot.</param>
        /// <param name="smoothingSeconds">Time constant of the pointer speed smoothing. 0 turns smoothing off.</param>
        public DragSwing(float maxAngle, float degreesPerSpeed, float frequency, float dampingRatio, float smoothingSeconds)
        {
            _maxAngle = Math.Max(0f, maxAngle);
            _degreesPerSpeed = degreesPerSpeed;
            _angularFrequency = 2f * (float)Math.PI * Math.Max(0f, frequency);
            _dampingRatio = Math.Max(0f, dampingRatio);
            _smoothingSeconds = Math.Max(0f, smoothingSeconds);
        }

        /// <summary>Sets the tilt to level and forgets the pointer speed.</summary>
        public void Reset()
        {
            Angle = 0f;
            _angleVelocity = 0f;
            _smoothedSpeed = 0f;
        }

        /// <summary>Advances the tilt by one frame and returns the new angle.</summary>
        /// <param name="pointerDeltaX">How far the pointer moved on the x axis since the last frame, in world units.</param>
        /// <param name="deltaTime">The frame time in seconds.</param>
        public float Step(float pointerDeltaX, float deltaTime)
        {
            if (deltaTime <= 0f)
                return Angle;

            float rawSpeed = pointerDeltaX / deltaTime;
            float blend = _smoothingSeconds > 0f ? 1f - (float)Math.Exp(-deltaTime / _smoothingSeconds) : 1f;
            _smoothedSpeed += (rawSpeed - _smoothedSpeed) * blend;

            float target = Clamp(_smoothedSpeed * _degreesPerSpeed);

            // Small fixed steps keep the spring stable and the result the same at any frame rate.
            float remaining = Math.Min(deltaTime, MaxFrameSeconds);
            int stepCount = (int)Math.Ceiling(remaining / MaxStepSeconds);
            float step = remaining / stepCount;

            for (int i = 0; i < stepCount; i++)
                Integrate(target, step);

            return Angle;
        }

        private void Integrate(float target, float step)
        {
            float acceleration = _angularFrequency * _angularFrequency * (target - Angle)
                - 2f * _dampingRatio * _angularFrequency * _angleVelocity;

            _angleVelocity += acceleration * step;
            Angle += _angleVelocity * step;

            if (Math.Abs(Angle) > _maxAngle)
            {
                Angle = Clamp(Angle);
                _angleVelocity = 0f;
            }
        }

        private float Clamp(float angle)
        {
            return Math.Max(-_maxAngle, Math.Min(_maxAngle, angle));
        }
    }
}
