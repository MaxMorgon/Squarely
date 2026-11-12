using NUnit.Framework;

namespace Squarely.Presentation.Tests
{
    public class VolumeFadeTests
    {
        [Test]
        public void FadeTo_MovesInAStraightLine()
        {
            var fade = new VolumeFade(0f);
            fade.FadeTo(1f, 2f);

            fade.Tick(0.5f);
            Assert.AreEqual(0.25f, fade.Current, 0.0001f);

            fade.Tick(10f);
            Assert.AreEqual(1f, fade.Current, 0.0001f);
        }

        [Test]
        public void FadeTo_ZeroSeconds_JumpsAtOnce()
        {
            var fade = new VolumeFade(1f);
            fade.FadeTo(0.2f, 0f);

            Assert.AreEqual(0.2f, fade.Current, 0.0001f);
        }

        [Test]
        public void FadeTo_NewTargetMidFade_MovesFromCurrent()
        {
            var fade = new VolumeFade(0f);
            fade.FadeTo(1f, 1f);
            fade.Tick(0.5f);
            fade.FadeTo(0f, 1f);
            fade.Tick(0.25f);

            Assert.AreEqual(0.375f, fade.Current, 0.0001f);
        }
    }
}
