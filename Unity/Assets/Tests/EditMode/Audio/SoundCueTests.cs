using NUnit.Framework;
using UnityEngine;

namespace Squarely.Presentation.Tests
{
    public class SoundCueTests
    {
        [Test]
        public void PickClip_NoClips_ReturnsNull()
        {
            var cue = ScriptableObject.CreateInstance<SoundCue>();

            Assert.IsNull(cue.PickClip(0.5f));

            Object.DestroyImmediate(cue);
        }

        [Test]
        public void PitchFor_DefaultCue_StaysInFivePercentAroundOne()
        {
            var cue = ScriptableObject.CreateInstance<SoundCue>();

            Assert.AreEqual(0.95f, cue.PitchFor(0, 0f), 0.0001f);
            Assert.AreEqual(1.05f, cue.PitchFor(0, 1f), 0.0001f);

            Object.DestroyImmediate(cue);
        }
    }
}
