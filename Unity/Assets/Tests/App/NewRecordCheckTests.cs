using NUnit.Framework;

namespace Squarely.App.Tests
{
    public sealed class NewRecordCheckTests
    {
        [Test]
        public void Check_NoBestScore_NeverCelebrates()
        {
            var check = new NewRecordCheck(null);

            Assert.That(check.Check(10), Is.False);
            Assert.That(check.Check(1000), Is.False);
        }

        [Test]
        public void Check_ScoreUpToBest_IsFalse()
        {
            var check = new NewRecordCheck(100);

            Assert.That(check.Check(50), Is.False);
            Assert.That(check.Check(100), Is.False);
        }

        [Test]
        public void Check_ScoreAboveBest_IsTrueOnce()
        {
            var check = new NewRecordCheck(100);

            Assert.That(check.Check(101), Is.True);
            Assert.That(check.Check(150), Is.False);
        }

        [Test]
        public void Check_BestZero_FirstPointsAreARecord()
        {
            var check = new NewRecordCheck(0);

            Assert.That(check.Check(0), Is.False);
            Assert.That(check.Check(10), Is.True);
        }

        [Test]
        public void Check_ScoreDropsBelowBestAfterRecord_StaysFalse()
        {
            var check = new NewRecordCheck(100);
            check.Check(120);

            Assert.That(check.Check(50), Is.False);
            Assert.That(check.Check(130), Is.False);
        }
    }
}
