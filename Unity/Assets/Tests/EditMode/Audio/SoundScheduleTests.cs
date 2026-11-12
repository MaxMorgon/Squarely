using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Squarely.Presentation.Tests
{
    public class SoundScheduleTests
    {
        /// <summary>A step that takes a fixed time and adds one sound at its start.</summary>
        private sealed class TimedSoundStep : CueStep
        {
            private readonly SoundCue _cue;
            private readonly float _seconds;

            public TimedSoundStep(SoundCue cue, float seconds)
            {
                _cue = cue;
                _seconds = seconds;
            }

            public override float PlannedSeconds => _seconds;

            public override Awaitable Play(CueTarget target, PlaybackContext context)
            {
                return Completed();
            }

            public override void Snap(CueTarget target, PlaybackContext context)
            {
            }

            public override void CollectSounds(float startSeconds, SoundLevelInput input, List<ScheduledSound> sounds)
            {
                sounds.Add(new ScheduledSound(startSeconds, _cue, input.ClearCount));
            }
        }

        [Test]
        public void Build_EachStepStartsWhenThePreviousEnds()
        {
            var cue = ScriptableObject.CreateInstance<SoundCue>();
            var steps = new CueStep[] { new TimedSoundStep(cue, 0.5f), new TimedSoundStep(cue, 0.25f), new TimedSoundStep(cue, 0f) };

            List<ScheduledSound> sounds = SoundSchedule.Build(steps, new SoundLevelInput(0, 2, 0));

            Assert.AreEqual(3, sounds.Count);
            Assert.AreEqual(0f, sounds[0].Seconds, 0.0001f);
            Assert.AreEqual(0.5f, sounds[1].Seconds, 0.0001f);
            Assert.AreEqual(0.75f, sounds[2].Seconds, 0.0001f);
            Assert.AreEqual(2, sounds[0].Level);

            Object.DestroyImmediate(cue);
        }

        [Test]
        public void Build_SkipsNullSteps()
        {
            var sounds = SoundSchedule.Build(new CueStep[] { null }, default);

            Assert.AreEqual(0, sounds.Count);
        }

        [Test]
        public void Build_WaitStep_DelaysTheNextStep()
        {
            var cue = ScriptableObject.CreateInstance<SoundCue>();
            var steps = new CueStep[] { new Wait(), new TimedSoundStep(cue, 0f) };

            List<ScheduledSound> sounds = SoundSchedule.Build(steps, default);

            Assert.AreEqual(new Wait().PlannedSeconds, sounds[0].Seconds, 0.0001f);
            Assert.Greater(sounds[0].Seconds, 0f);

            Object.DestroyImmediate(cue);
        }
    }
}
