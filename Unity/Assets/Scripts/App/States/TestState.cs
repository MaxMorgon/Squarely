using System;
using System.Collections.Generic;
using Squarely.Presentation;
using Squarely.Simulation;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Squarely.App
{
    /// <summary>
    /// The test screen, for replaying the same game: a seed field that keeps the last seed played, Randomize,
    /// a high score field (0 each time the screen opens), the "start in test screen" toggle, Play and Back.
    /// Play starts the default game mode with exactly that seed and saves it; the game plays against the field's
    /// high score and never saves its high score on the device. Back returns to the menu. The game it starts returns here when the player quits or leaves the game over screen.
    /// Play is unavailable while a field holds no valid value. The toggle is saved as soon as it changes.
    /// The replays section lists the replay history (newest first) with Play and Copy per row, and has a field to paste
    /// a replay code. Play opens the replay watch (<see cref="ReplayState"/>) with the default game mode; Back there returns here.
    /// A pasted code shows an error if it cannot be read, or a warning if it was recorded with a different game config (Play still works).
    /// Pasted Play is unavailable while the field is empty.
    /// </summary>
    public sealed class TestState : IAppState
    {
        private const int StartHighScore = 0;

        private readonly AppContext _context;
        private readonly AppStateMachine _stateMachine;

        private TestScreen _screen;
        private ulong? _seed;
        private int? _highScore;
        private IReadOnlyList<ReplayHistoryEntry> _historyEntries;
        private ReplayPasteResult _pasteResult;

        public TestState(AppContext context, AppStateMachine stateMachine)
        {
            _context = context;
            _stateMachine = stateMachine;
        }

        public void Enter()
        {
            _screen = _context.CreateScreen(_context.ScreenPrefabs.Test);
            _screen.Initialize(_context.Texts.Test);
            _screen.SetStartInTest(_context.StartInTest.IsOn);
            _highScore = StartHighScore;
            _screen.SetHighScore(TestHighScore.Format(StartHighScore));
            SetSeed(_context.TestSeed.LoadOrCreate());
            ShowHistory();
            _screen.SetReplayCodePlayable(false);

            _screen.ReplayCodeEditEvent += OnReplayCodeEdited;
            _screen.ReplayCodePlayRequestEvent += OnReplayCodePlayRequested;
            _screen.HistoryPlayRequestEvent += OnHistoryPlayRequested;
            _screen.HistoryCopyRequestEvent += OnHistoryCopyRequested;
            _screen.SeedEditEvent += OnSeedEdited;
            _screen.HighScoreEditEvent += OnHighScoreEdited;
            _screen.StartInTestChangeEvent += OnStartInTestChanged;
            _screen.RandomizeRequestEvent += OnRandomizeRequested;
            _screen.PlayRequestEvent += OnPlayRequested;
            _screen.BackRequestEvent += OnBackRequested;
        }

        public void Exit()
        {
            _screen.ReplayCodeEditEvent -= OnReplayCodeEdited;
            _screen.ReplayCodePlayRequestEvent -= OnReplayCodePlayRequested;
            _screen.HistoryPlayRequestEvent -= OnHistoryPlayRequested;
            _screen.HistoryCopyRequestEvent -= OnHistoryCopyRequested;
            _screen.SeedEditEvent -= OnSeedEdited;
            _screen.HighScoreEditEvent -= OnHighScoreEdited;
            _screen.StartInTestChangeEvent -= OnStartInTestChanged;
            _screen.RandomizeRequestEvent -= OnRandomizeRequested;
            _screen.PlayRequestEvent -= OnPlayRequested;
            _screen.BackRequestEvent -= OnBackRequested;
            Object.Destroy(_screen.gameObject);
        }

        private void SetSeed(ulong seed)
        {
            _seed = seed;
            _screen.SetSeed(TestSeed.Format(seed));
            UpdatePlayable();
        }

        private void UpdatePlayable()
        {
            _screen.SetPlayable(_seed.HasValue && _highScore.HasValue);
        }

        #region Replays

        private void ShowHistory()
        {
            TestTexts texts = _context.Texts.Test;
            var rows = new List<string>();
            _historyEntries = _context.ReplayHistory.Entries;

            foreach (ReplayHistoryEntry entry in _historyEntries)
            {
                DateTime localTime = entry.SavedAtUtc.ToLocalTime();
                rows.Add(ReplayRowFormat.Format(entry, localTime, texts.ReplayRowFormat, texts.ReplayFinished, texts.ReplayUnfinished));
            }

            _screen.SetHistory(rows);
        }

        private void OnReplayCodeEdited(string text)
        {
            ReplayPasteResult result = ReplayPaste.Check(text, _context.DefaultConfig);
            _screen.SetReplayCodePlayable(result.Status != ReplayPasteStatus.Empty);
            _screen.SetReplayMessage(MessageFor(result));
            _pasteResult = result;
        }

        private void OnReplayCodePlayRequested()
        {
            if (_pasteResult != null && _pasteResult.CanPlay)
                PlayReplay(_pasteResult.Replay);
        }

        private void OnHistoryPlayRequested(int index)
        {
            ReplayPasteResult result = ReplayPaste.Check(_historyEntries[index].Code, _context.DefaultConfig);

            if (!result.CanPlay)
            {
                _screen.SetReplayMessage(MessageFor(result));
                return;
            }

            PlayReplay(result.Replay);
        }

        private void OnHistoryCopyRequested(int index)
        {
            new SystemClipboard().SetText(_historyEntries[index].Code);
            _screen.SetReplayMessage(_context.Texts.Test.ReplayCopied);
        }

        private void PlayReplay(GameReplay replay)
        {
            _stateMachine.ChangeState(new ReplayState(_context, _stateMachine, replay, () => new TestState(_context, _stateMachine)));
        }

        /// <summary>The text for a pasted code's check: the error, the config warning, or null.</summary>
        private string MessageFor(ReplayPasteResult result)
        {
            TestTexts texts = _context.Texts.Test;

            switch (result.Status)
            {
                case ReplayPasteStatus.Invalid:
                    return string.Format(texts.PasteInvalidFormat, result.Error);
                case ReplayPasteStatus.ConfigMismatch:
                    return texts.ConfigWarning;
                default:
                    return null;
            }
        }

        #endregion

        #region Screen events

        private void OnSeedEdited(string text)
        {
            _seed = TestSeed.TryParse(text, out ulong seed) ? seed : (ulong?)null;
            UpdatePlayable();
        }

        private void OnHighScoreEdited(string text)
        {
            _highScore = TestHighScore.TryParse(text, out int highScore) ? highScore : (int?)null;
            UpdatePlayable();
        }

        private void OnStartInTestChanged(bool isOn)
        {
            _context.StartInTest.Set(isOn);
        }

        private void OnRandomizeRequested()
        {
            SetSeed(_context.TestSeed.CreateNew());
        }

        private void OnPlayRequested()
        {
            if (!_seed.HasValue || !_highScore.HasValue)
                return;

            _context.TestSeed.Save(_seed.Value);
            HighScore highScore = TestHighScore.Create(_highScore.Value);
            _stateMachine.ChangeState(new GameState(_context, _stateMachine, _context.DefaultConfig, _seed.Value, highScore, () => new TestState(_context, _stateMachine)));
        }

        private void OnBackRequested()
        {
            _stateMachine.ChangeState(new MenuState(_context, _stateMachine));
        }

        #endregion
    }
}
