using UnityEngine;

namespace Squarely.Presentation.Tests
{
    /// <summary>A cue step that counts its calls. It either completes as soon as it plays, or waits for <see cref="Complete"/>.</summary>
    public sealed class FakeStep : CueStep
    {
        private readonly bool _completesAtOnce;
        private AwaitableCompletionSource _pending;

        public int PlayCount { get; private set; }

        public int SnapCount { get; private set; }

        public FakeStep(bool completesAtOnce)
        {
            _completesAtOnce = completesAtOnce;
        }

        public override Awaitable Play(CueTarget target, PlaybackContext context)
        {
            PlayCount++;
            _pending = new AwaitableCompletionSource();

            if (_completesAtOnce)
                _pending.SetResult();

            return _pending.Awaitable;
        }

        public override void Snap(CueTarget target, PlaybackContext context)
        {
            SnapCount++;
        }

        /// <summary>Ends a step that is waiting.</summary>
        public void Complete()
        {
            _pending.TrySetResult();
        }
    }
}
