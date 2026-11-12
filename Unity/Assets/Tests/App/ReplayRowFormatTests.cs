using System;
using NUnit.Framework;

namespace Squarely.App.Tests
{
    public sealed class ReplayRowFormatTests
    {
        private const string Format = "{0:yyyy-MM-dd HH:mm} S{1} M{2} {3}";

        private static readonly DateTime Time = new DateTime(2026, 3, 4, 15, 7, 0);

        [Test]
        public void Format_Finished_UsesFinishedText()
        {
            var entry = new ReplayHistoryEntry("SQ1.x", Time, 120, 14, true);

            Assert.That(ReplayRowFormat.Format(entry, Time, Format, "done", "left"), Is.EqualTo("2026-03-04 15:07 S120 M14 done"));
        }

        [Test]
        public void Format_Unfinished_UsesUnfinishedText()
        {
            var entry = new ReplayHistoryEntry("SQ1.x", Time, 5, 2, false);

            Assert.That(ReplayRowFormat.Format(entry, Time, Format, "done", "left"), Is.EqualTo("2026-03-04 15:07 S5 M2 left"));
        }
    }
}
