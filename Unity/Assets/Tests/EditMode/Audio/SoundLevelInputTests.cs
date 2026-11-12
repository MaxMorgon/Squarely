using System.Collections.Generic;
using NUnit.Framework;
using Squarely.Simulation;

namespace Squarely.Presentation.Tests
{
    public class SoundLevelInputTests
    {
        private static PatternsClearedEvent Cleared(int matchCount)
        {
            var matches = new List<ClearedMatch>();

            for (int i = 0; i < matchCount; i++)
                matches.Add(new ClearedMatch(null, new List<GridPos>()));

            return new PatternsClearedEvent(matches, new List<GridPos>());
        }

        private static CueTarget CountTarget(int to)
        {
            return CueTarget.ForCount(0, to, _ => { }, null, UnityEngine.Vector3.zero);
        }

        [Test]
        public void From_CountsMatchesOverAllClearEvents()
        {
            var events = new List<ISimEvent> { Cleared(2), Cleared(1) };

            SoundLevelInput input = SoundLevelInput.From(CountTarget(0), events);

            Assert.AreEqual(3, input.ClearCount);
        }

        [Test]
        public void From_ComboLevelIsTheLastCombo()
        {
            var events = new List<ISimEvent> { new ComboChangedEvent(0, 1), new ComboChangedEvent(1, 2) };

            SoundLevelInput input = SoundLevelInput.From(CountTarget(0), events);

            Assert.AreEqual(2, input.ComboLevel);
        }

        [Test]
        public void From_CountIsTheTargetEnd_ZeroWithoutCount()
        {
            var events = new List<ISimEvent>();

            Assert.AreEqual(40, SoundLevelInput.From(CountTarget(40), events).Count);
            Assert.AreEqual(0, SoundLevelInput.From(CueTarget.ForTransform(null, UnityEngine.Vector3.zero), events).Count);
        }

        [Test]
        public void Select_ReturnsTheNumberOfTheSource()
        {
            var input = new SoundLevelInput(5, 3, 2, 4);

            Assert.AreEqual(0, input.Select(SoundLevelSource.None));
            Assert.AreEqual(5, input.Select(SoundLevelSource.Count));
            Assert.AreEqual(3, input.Select(SoundLevelSource.ClearCount));
            Assert.AreEqual(2, input.Select(SoundLevelSource.ComboLevel));
            Assert.AreEqual(4, input.Select(SoundLevelSource.ComboLost));
        }

        [Test]
        public void From_ComboLostIsThePreviousComboWhenItDropsToZero()
        {
            var events = new List<ISimEvent> { new ComboChangedEvent(3, 0) };

            SoundLevelInput input = SoundLevelInput.From(CountTarget(0), events);

            Assert.AreEqual(3, input.ComboLostLevel);
            Assert.AreEqual(0, input.ComboLevel);
        }

        [Test]
        public void From_ComboLostIsZeroWhenTheComboGrowsOrDoesNotChange()
        {
            Assert.AreEqual(0, SoundLevelInput.From(CountTarget(0), new List<ISimEvent> { new ComboChangedEvent(1, 2) }).ComboLostLevel);
            Assert.AreEqual(0, SoundLevelInput.From(CountTarget(0), new List<ISimEvent> { new ComboChangedEvent(0, 1) }).ComboLostLevel);
            Assert.AreEqual(0, SoundLevelInput.From(CountTarget(0), new List<ISimEvent>()).ComboLostLevel);
        }

        [Test]
        public void From_CountFromIsTheTargetStart()
        {
            var target = CueTarget.ForCount(7, 40, _ => { }, null, UnityEngine.Vector3.zero);

            Assert.AreEqual(7, SoundLevelInput.From(target, new List<ISimEvent>()).CountFrom);
        }
    }
}
