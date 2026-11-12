using NUnit.Framework;

namespace Squarely.Presentation.Tests
{
    public class MusicSwitchRuleTests
    {
        [Test]
        public void Choose_ActiveTrackNotFadingOut_Keeps()
        {
            Assert.AreEqual(MusicSwitchRule.Action.Keep, MusicSwitchRule.Choose(true, true, 1f, false, false));
        }

        [Test]
        public void Choose_ActiveTrackFadingOut_FadesActiveBackIn()
        {
            Assert.AreEqual(MusicSwitchRule.Action.FadeActiveBackIn, MusicSwitchRule.Choose(true, true, 0f, false, false));
        }

        [Test]
        public void Choose_TrackStillPlaysOnOtherSource_FadesOtherBackIn()
        {
            Assert.AreEqual(MusicSwitchRule.Action.FadeOtherBackIn, MusicSwitchRule.Choose(false, true, 1f, true, true));
        }

        [Test]
        public void Choose_TrackStopped_StartsNew()
        {
            Assert.AreEqual(MusicSwitchRule.Action.StartNew, MusicSwitchRule.Choose(true, false, 0f, false, false));
            Assert.AreEqual(MusicSwitchRule.Action.StartNew, MusicSwitchRule.Choose(false, true, 1f, true, false));
            Assert.AreEqual(MusicSwitchRule.Action.StartNew, MusicSwitchRule.Choose(false, false, 0f, false, false));
        }
    }
}
