using NUnit.Framework;

namespace Squarely.App.Tests
{
    public sealed class ReplayStepperTests
    {
        private const float Delay = 0.4f;

        [Test]
        public void Tick_NotDue_BeforeDelayPassed()
        {
            var stepper = new ReplayStepper(3, Delay);

            Assert.That(stepper.Tick(0.3f, false), Is.False);
        }

        [Test]
        public void Tick_Due_AfterDelayPassedInSeveralTicks()
        {
            var stepper = new ReplayStepper(3, Delay);

            stepper.Tick(0.3f, false);

            Assert.That(stepper.Tick(0.2f, false), Is.True);
        }

        [Test]
        public void Tick_NotDue_WhilePlaybackRuns()
        {
            var stepper = new ReplayStepper(3, Delay);

            Assert.That(stepper.Tick(5f, true), Is.False);
        }

        [Test]
        public void Tick_DelayRestarts_WhenPlaybackRuns()
        {
            var stepper = new ReplayStepper(3, Delay);
            stepper.Tick(0.3f, false);
            stepper.Tick(0.1f, true);

            Assert.That(stepper.Tick(0.3f, false), Is.False);
        }

        [Test]
        public void Tick_NotDue_WhilePaused()
        {
            var stepper = new ReplayStepper(3, Delay);
            stepper.SetPaused(true);

            Assert.That(stepper.Tick(5f, false), Is.False);
        }

        [Test]
        public void Tick_DelayRestarts_AfterResume()
        {
            var stepper = new ReplayStepper(3, Delay);
            stepper.Tick(0.3f, false);
            stepper.SetPaused(true);
            stepper.SetPaused(false);

            Assert.That(stepper.Tick(0.3f, false), Is.False);
        }

        [Test]
        public void MarkExecuted_CountsAndRestartsDelay()
        {
            var stepper = new ReplayStepper(3, Delay);
            stepper.Tick(0.39f, false);

            stepper.MarkExecuted();

            Assert.That(stepper.Executed, Is.EqualTo(1));
            Assert.That(stepper.Tick(0.1f, false), Is.False);
        }

        [Test]
        public void HasNext_False_AfterAllExecuted()
        {
            var stepper = new ReplayStepper(1, Delay);

            stepper.MarkExecuted();

            Assert.That(stepper.HasNext, Is.False);
            Assert.That(stepper.Tick(5f, false), Is.False);
        }

        [Test]
        public void HasNext_False_WithNoCommands()
        {
            Assert.That(new ReplayStepper(0, Delay).HasNext, Is.False);
        }

        [Test]
        public void Stop_EndsReplayEarly()
        {
            var stepper = new ReplayStepper(3, Delay);

            stepper.Stop();

            Assert.That(stepper.HasNext, Is.False);
            Assert.That(stepper.Tick(5f, false), Is.False);
        }

        [Test]
        public void CanStepBack_False_AtTheStart()
        {
            var stepper = new ReplayStepper(3, Delay);

            Assert.That(stepper.CanStepBack, Is.False);
            Assert.That(stepper.StepBack(), Is.False);
            Assert.That(stepper.Executed, Is.EqualTo(0));
        }

        [Test]
        public void StepBack_UncountsTheLastCommand()
        {
            var stepper = new ReplayStepper(3, Delay);
            stepper.MarkExecuted();
            stepper.MarkExecuted();

            bool didStepBack = stepper.StepBack();

            Assert.That(didStepBack, Is.True);
            Assert.That(stepper.Executed, Is.EqualTo(1));
            Assert.That(stepper.CanStepBack, Is.True);
        }

        [Test]
        public void StepBack_PausesAutoPlay()
        {
            var stepper = new ReplayStepper(3, Delay);
            stepper.MarkExecuted();

            stepper.StepBack();

            Assert.That(stepper.IsPaused, Is.True);
            Assert.That(stepper.Tick(5f, false), Is.False);
        }

        [Test]
        public void StepBack_AfterAllExecuted_HasNextAgain()
        {
            var stepper = new ReplayStepper(2, Delay);
            stepper.MarkExecuted();
            stepper.MarkExecuted();

            stepper.StepBack();

            Assert.That(stepper.HasNext, Is.True);
        }

        [Test]
        public void StepBack_AfterStop_LiftsTheStop()
        {
            var stepper = new ReplayStepper(3, Delay);
            stepper.MarkExecuted();
            stepper.Stop();

            stepper.StepBack();

            Assert.That(stepper.HasNext, Is.True);
        }

        [Test]
        public void StepBack_ThenMarkExecuted_CountsAgain()
        {
            var stepper = new ReplayStepper(3, Delay);
            stepper.MarkExecuted();
            stepper.StepBack();

            stepper.MarkExecuted();

            Assert.That(stepper.Executed, Is.EqualTo(1));
        }
    }
}
