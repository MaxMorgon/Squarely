using System.Collections;
using UnityEngine;

namespace Squarely.Presentation
{
    /// <summary>
    /// Animates its transform's scale along a curve when played, e.g. a punch on a UI label.
    /// </summary>
    public sealed class ScalePunch : MonoBehaviour
    {
        [SerializeField] private float _duration = 0.4f;

        [Tooltip("Scale over the animation (time 0 to 1). End at 1 to leave the transform at its normal size.")]
        [SerializeField] private AnimationCurve _scale = new AnimationCurve(
            new Keyframe(0f, 1f),
            new Keyframe(0.35f, 1.35f),
            new Keyframe(1f, 1f));

        private Coroutine _running;

        /// <summary>Plays the animation from the start, restarting it if it is already playing.</summary>
        public void Play()
        {
            if (_running != null)
                StopCoroutine(_running);

            _running = StartCoroutine(Run());
        }

        private IEnumerator Run()
        {
            float elapsed = 0f;

            while (elapsed < _duration)
            {
                SetScale(elapsed / _duration);
                yield return null;
                elapsed += Time.deltaTime;
            }

            SetScale(1f);
            _running = null;
        }

        private void SetScale(float t)
        {
            transform.localScale = Vector3.one * _scale.Evaluate(t);
        }
    }
}
