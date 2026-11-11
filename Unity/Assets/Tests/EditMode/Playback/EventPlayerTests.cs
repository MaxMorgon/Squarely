using System.Threading;
using NUnit.Framework;
using Squarely.Simulation;

namespace Squarely.Presentation.Tests
{
    public class EventPlayerTests
    {
        private EventPlayer _player;
        private int _endCount;

        [SetUp]
        public void SetUp()
        {
            _player = new EventPlayer(null, CancellationToken.None);
            _player.AddHandler(new FakeEventHandler());
            _player.PlaybackEndEvent += () => _endCount++;
            _endCount = 0;
        }

        [Test]
        public void Play_StepsThatEndAtOnce_PlaysEveryEvent_AndEnds()
        {
            var first = new FakeStep(true);
            var second = new FakeStep(true);

            _player.Play(new ISimEvent[] { new FakeEvent(true, first), new FakeEvent(true, second) });

            Assert.That(first.PlayCount, Is.EqualTo(1));
            Assert.That(second.PlayCount, Is.EqualTo(1));
            Assert.That(_endCount, Is.EqualTo(1));
            Assert.That(_player.IsPlaying, Is.False);
        }

        [Test]
        public void Play_WaitingStep_LaterEventsWait()
        {
            var running = new FakeStep(false);
            var later = new FakeEvent(true, new FakeStep(true));

            _player.Play(new ISimEvent[] { new FakeEvent(true, running), later });

            Assert.That(running.PlayCount, Is.EqualTo(1));
            Assert.That(later.HandleCount, Is.EqualTo(0));
            Assert.That(_player.IsPlaying, Is.True);
            Assert.That(_endCount, Is.EqualTo(0));
        }

        [Test]
        public void Play_SequenceThatDoesNotWait_NextEventStartsAlongside()
        {
            var running = new FakeStep(false);
            var next = new FakeStep(true);

            _player.Play(new ISimEvent[] { new FakeEvent(false, running), new FakeEvent(true, next) });

            Assert.That(next.PlayCount, Is.EqualTo(1));
            Assert.That(_player.IsPlaying, Is.True);
        }

        [Test]
        public void Snap_SnapsRunningAndUnstartedSteps_AndHandlesRemainingEvents()
        {
            var running = new FakeStep(false);
            var unstarted = new FakeStep(true);
            var laterStep = new FakeStep(true);
            var first = new FakeEvent(true, running, unstarted);
            var later = new FakeEvent(true, laterStep);
            _player.Play(new ISimEvent[] { first, later });

            _player.Snap();

            Assert.That(running.SnapCount, Is.EqualTo(1));
            Assert.That(unstarted.PlayCount, Is.EqualTo(0));
            Assert.That(unstarted.SnapCount, Is.EqualTo(1));
            Assert.That(later.HandleCount, Is.EqualTo(1));
            Assert.That(laterStep.PlayCount, Is.EqualTo(0));
            Assert.That(laterStep.SnapCount, Is.EqualTo(1));
            Assert.That(first.RunEndCount, Is.EqualTo(1));
            Assert.That(later.RunEndCount, Is.EqualTo(1));
            Assert.That(_endCount, Is.EqualTo(1));
            Assert.That(_player.IsPlaying, Is.False);
        }

        [Test]
        public void Snap_ThenSnappedStepCompletes_NothingPlaysOrEndsAgain()
        {
            var running = new FakeStep(false);
            var unstarted = new FakeStep(true);
            var first = new FakeEvent(true, running, unstarted);
            _player.Play(new ISimEvent[] { first });
            _player.Snap();

            running.Complete();

            Assert.That(unstarted.PlayCount, Is.EqualTo(0));
            Assert.That(first.RunEndCount, Is.EqualTo(1));
            Assert.That(_endCount, Is.EqualTo(1));
        }

        [Test]
        public void Snap_AlreadyPlayedStep_IsNotSnapped()
        {
            var played = new FakeStep(true);
            var running = new FakeStep(false);
            _player.Play(new ISimEvent[] { new FakeEvent(true, played, running) });

            _player.Snap();

            Assert.That(played.SnapCount, Is.EqualTo(0));
            Assert.That(running.SnapCount, Is.EqualTo(1));
        }

        [Test]
        public void Snap_NothingPlaying_DoesNothing()
        {
            _player.Snap();

            Assert.That(_endCount, Is.EqualTo(0));
        }

        [Test]
        public void Play_WhilePlaying_SnapsPreviousPlaybackFirst()
        {
            var running = new FakeStep(false);
            var next = new FakeStep(true);
            _player.Play(new ISimEvent[] { new FakeEvent(true, running) });

            _player.Play(new ISimEvent[] { new FakeEvent(true, next) });

            Assert.That(running.SnapCount, Is.EqualTo(1));
            Assert.That(next.PlayCount, Is.EqualTo(1));
            Assert.That(_endCount, Is.EqualTo(2));
        }

        [Test]
        public void Play_EventWithoutHandler_IsSkipped()
        {
            var step = new FakeStep(true);

            _player.Play(new ISimEvent[] { new ComboChangedEvent(0, 1), new FakeEvent(true, step) });

            Assert.That(step.PlayCount, Is.EqualTo(1));
            Assert.That(_endCount, Is.EqualTo(1));
        }
    }
}
