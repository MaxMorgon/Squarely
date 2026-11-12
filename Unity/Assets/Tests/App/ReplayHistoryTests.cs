using System;
using System.Collections.Generic;
using NUnit.Framework;
using Squarely.Simulation;

namespace Squarely.App.Tests
{
    public sealed class ReplayHistoryTests
    {
        private MemoryKeyValueStore _store;
        private DateTime _now;
        private ReplayHistory _history;

        [SetUp]
        public void SetUp()
        {
            _store = new MemoryKeyValueStore();
            _now = new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);
            _history = new ReplayHistory(_store, () => _now);
        }

        private static GameReplay Replay(ulong seed, int moves, int score, bool resolved)
        {
            var commands = new List<ISimCommand>();

            for (int i = 0; i < moves; i++)
                commands.Add(new PlacePieceCommand(i, new GridPos(0, 0)));

            if (resolved)
                commands.Add(new EndGameCommand());

            return new GameReplay(seed, 1UL, commands, score, resolved);
        }

        [Test]
        public void Entries_Empty_BeforeFirstRecord()
        {
            Assert.That(_history.Entries, Is.Empty);
        }

        [Test]
        public void Record_SavesCodeTimeScoreMovesAndFinished()
        {
            GameReplay replay = Replay(5UL, 3, 40, true);

            bool saved = _history.Record(replay);

            Assert.That(saved, Is.True);
            ReplayHistoryEntry entry = _history.Entries[0];
            Assert.That(entry.Code, Is.EqualTo(ReplayCodec.Encode(replay)));
            Assert.That(entry.SavedAtUtc, Is.EqualTo(_now));
            Assert.That(entry.Score, Is.EqualTo(40));
            Assert.That(entry.MoveCount, Is.EqualTo(3));
            Assert.That(entry.IsFinished, Is.True);
            Assert.That(_store.SaveCount, Is.EqualTo(1));
        }

        [Test]
        public void Record_AbandonedWithMoves_IsSavedAsNotFinished()
        {
            Assert.That(_history.Record(Replay(5UL, 2, 10, false)), Is.True);
            Assert.That(_history.Entries[0].IsFinished, Is.False);
        }

        [Test]
        public void Record_AbandonedWithoutMoves_IsNotSaved()
        {
            Assert.That(_history.Record(Replay(5UL, 0, 0, false)), Is.False);
            Assert.That(_history.Entries, Is.Empty);
        }

        [Test]
        public void Record_FinishedWithoutMoves_IsSaved()
        {
            Assert.That(_history.Record(Replay(5UL, 0, 0, true)), Is.True);
            Assert.That(_history.Entries.Count, Is.EqualTo(1));
        }

        [Test]
        public void Record_NewestFirst_AndDropsOldestPastCapacity()
        {
            for (int i = 0; i < ReplayHistory.Capacity + 2; i++)
            {
                _now = _now.AddMinutes(1);
                _history.Record(Replay((ulong)i, 1, i, false));
            }

            Assert.That(_history.Entries.Count, Is.EqualTo(ReplayHistory.Capacity));
            Assert.That(_history.Entries[0].Score, Is.EqualTo(ReplayHistory.Capacity + 1));
            Assert.That(_history.Entries[ReplayHistory.Capacity - 1].Score, Is.EqualTo(2));
        }

        [Test]
        public void Entries_SurviveANewHistoryOnTheSameStore()
        {
            _history.Record(Replay(5UL, 1, 9, false));

            var reloaded = new ReplayHistory(_store, () => _now);

            Assert.That(reloaded.Entries.Count, Is.EqualTo(1));
            Assert.That(reloaded.Entries[0].Score, Is.EqualTo(9));
        }

        [Test]
        public void Entries_BadEntriesAreSkipped()
        {
            _history.Record(Replay(5UL, 1, 9, false));
            string good = _store.GetString(ReplayHistory.StoreKey, "");
            string bad = "garbage\n1|2|3\nx|1|1|1|SQ1.a\n1|1|1|7|SQ1.a\n1|1|-1|1|SQ1.a\n1|1|1|1|\n";
            _store.SetString(ReplayHistory.StoreKey, bad + good);

            Assert.That(_history.Entries.Count, Is.EqualTo(1));
            Assert.That(_history.Entries[0].Score, Is.EqualTo(9));
        }

        [Test]
        public void Record_AfterCorruptData_StillSaves()
        {
            _store.SetString(ReplayHistory.StoreKey, "nonsense||||");

            Assert.That(_history.Record(Replay(5UL, 1, 9, false)), Is.True);
            Assert.That(_history.Entries.Count, Is.EqualTo(1));
        }

        [Test]
        public void Entries_StoredAsInt_IsEmpty()
        {
            _store.SetInt(ReplayHistory.StoreKey, 3);

            Assert.That(_history.Entries, Is.Empty);
        }
    }
}
