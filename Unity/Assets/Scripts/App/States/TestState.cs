using Squarely.Presentation;
using UnityEngine;

namespace Squarely.App
{
    /// <summary>
    /// The test screen, for replaying the same game: a seed field that keeps the last seed played, Randomize,
    /// a high score field (0 each time the screen opens), the "start in test screen" toggle, Play and Back.
    /// Play starts the default game mode with exactly that seed and saves it; the game plays against the field's
    /// high score and never saves its high score on the device. Back returns to the menu. The game it starts returns here when the player quits or leaves the game over screen.
    /// Play is unavailable while a field holds no valid value. The toggle is saved as soon as it changes.
    /// </summary>
    public sealed class TestState : IAppState
    {
        private const int StartHighScore = 0;

        private readonly AppContext _context;
        private readonly AppStateMachine _stateMachine;

        private TestScreen _screen;
        private ulong? _seed;
        private int? _highScore;

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

            _screen.SeedEditEvent += OnSeedEdited;
            _screen.HighScoreEditEvent += OnHighScoreEdited;
            _screen.StartInTestChangeEvent += OnStartInTestChanged;
            _screen.RandomizeRequestEvent += OnRandomizeRequested;
            _screen.PlayRequestEvent += OnPlayRequested;
            _screen.BackRequestEvent += OnBackRequested;
        }

        public void Exit()
        {
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
