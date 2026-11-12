using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Squarely.Simulation;

namespace Squarely.App
{
    /// <summary>
    /// The last games played on this device, saved in a store as replay codes. Newest first.
    /// Keeps at most <see cref="Capacity"/> entries and drops the oldest. Bad stored data is skipped.
    /// </summary>
    public sealed class ReplayHistory
    {
        /// <summary>The key the history is saved under.</summary>
        public const string StoreKey = "ReplayHistory";

        /// <summary>The most entries the history keeps.</summary>
        public const int Capacity = 10;

        private const char FieldSeparator = '|';
        private const char EntrySeparator = '\n';
        private const int FieldCount = 5;

        private readonly IKeyValueStore _store;
        private readonly Func<DateTime> _utcNow;

        /// <summary>The saved games, newest first. Read from the store on each call.</summary>
        public IReadOnlyList<ReplayHistoryEntry> Entries => Load();

        /// <param name="utcNow">The clock for the save time.</param>
        public ReplayHistory(IKeyValueStore store, Func<DateTime> utcNow)
        {
            _store = store ?? throw new ArgumentNullException(nameof(store));
            _utcNow = utcNow ?? throw new ArgumentNullException(nameof(utcNow));
        }

        /// <summary>
        /// Saves the game as the newest entry. Returns false and saves nothing for a game that is not
        /// finished and has no placed piece. A finished game is always saved.
        /// </summary>
        public bool Record(GameReplay replay)
        {
            if (replay == null)
                throw new ArgumentNullException(nameof(replay));

            int moveCount = CountMoves(replay);

            if (!replay.EndIsResolved && moveCount == 0)
                return false;

            string code = ReplayCodec.Encode(replay);
            var entry = new ReplayHistoryEntry(code, _utcNow(), replay.EndScore, moveCount, replay.EndIsResolved);

            List<ReplayHistoryEntry> entries = Load();
            entries.Insert(0, entry);

            if (entries.Count > Capacity)
                entries.RemoveRange(Capacity, entries.Count - Capacity);

            _store.SetString(StoreKey, Serialize(entries));
            _store.Save();
            return true;
        }

        private static int CountMoves(GameReplay replay)
        {
            int count = 0;

            foreach (ISimCommand command in replay.Commands)
            {
                if (command is PlacePieceCommand)
                    count++;
            }

            return count;
        }

        #region Storage format

        // One entry per line: ticks|score|moves|finished|code.

        private static string Serialize(List<ReplayHistoryEntry> entries)
        {
            var builder = new StringBuilder();

            foreach (ReplayHistoryEntry entry in entries)
            {
                if (builder.Length > 0)
                    builder.Append(EntrySeparator);

                builder.Append(entry.SavedAtUtc.Ticks.ToString(CultureInfo.InvariantCulture)).Append(FieldSeparator);
                builder.Append(entry.Score.ToString(CultureInfo.InvariantCulture)).Append(FieldSeparator);
                builder.Append(entry.MoveCount.ToString(CultureInfo.InvariantCulture)).Append(FieldSeparator);
                builder.Append(entry.IsFinished ? '1' : '0').Append(FieldSeparator);
                builder.Append(entry.Code);
            }

            return builder.ToString();
        }

        private List<ReplayHistoryEntry> Load()
        {
            var entries = new List<ReplayHistoryEntry>();
            string text = _store.GetString(StoreKey, string.Empty);

            if (string.IsNullOrEmpty(text))
                return entries;

            foreach (string line in text.Split(EntrySeparator))
            {
                if (TryParseEntry(line, out ReplayHistoryEntry entry))
                    entries.Add(entry);
            }

            if (entries.Count > Capacity)
                entries.RemoveRange(Capacity, entries.Count - Capacity);

            return entries;
        }

        private static bool TryParseEntry(string line, out ReplayHistoryEntry entry)
        {
            entry = null;
            string[] fields = line.Split(FieldSeparator);

            if (fields.Length != FieldCount)
                return false;

            NumberStyles style = NumberStyles.Integer;
            CultureInfo culture = CultureInfo.InvariantCulture;

            if (!long.TryParse(fields[0], style, culture, out long ticks)
                || ticks < DateTime.MinValue.Ticks || ticks > DateTime.MaxValue.Ticks
                || !int.TryParse(fields[1], style, culture, out int score)
                || !int.TryParse(fields[2], style, culture, out int moveCount)
                || moveCount < 0
                || (fields[3] != "0" && fields[3] != "1")
                || string.IsNullOrWhiteSpace(fields[4]))
            {
                return false;
            }

            entry = new ReplayHistoryEntry(fields[4], new DateTime(ticks, DateTimeKind.Utc), score, moveCount, fields[3] == "1");
            return true;
        }

        #endregion
    }
}
