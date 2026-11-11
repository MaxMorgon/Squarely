using Squarely.Presentation;
using UnityEngine;

namespace Squarely.App
{
    /// <summary>
    /// The test screen, for replaying the same game: a seed field that keeps the last seed played, Randomize, Play and Back.
    /// Play starts the default game mode with exactly that seed and saves it; Back returns to the menu.
    /// Play is unavailable while the field holds no valid seed.
    /// </summary>
    public sealed class TestState : IAppState
    {
        private readonly AppContext _context;
        private readonly AppStateMachine _stateMachine;

        private TestScreen _screen;
        private ulong? _seed;

        public TestState(AppContext context, AppStateMachine stateMachine)
        {
            _context = context;
            _stateMachine = stateMachine;
        }

        public void Enter()
        {
            _screen = Object.Instantiate(_context.ScreenPrefabs.Test, _context.ScreenRoot);
            _screen.Initialize(_context.Texts.Test);
            SetSeed(_context.TestSeeds.LoadOrCreate());

            _screen.SeedEditEvent += OnSeedEdited;
            _screen.RandomizeRequestEvent += OnRandomizeRequested;
            _screen.PlayRequestEvent += OnPlayRequested;
            _screen.BackRequestEvent += OnBackRequested;
        }

        public void Exit()
        {
            _screen.SeedEditEvent -= OnSeedEdited;
            _screen.RandomizeRequestEvent -= OnRandomizeRequested;
            _screen.PlayRequestEvent -= OnPlayRequested;
            _screen.BackRequestEvent -= OnBackRequested;
            Object.Destroy(_screen.gameObject);
        }

        private void SetSeed(ulong seed)
        {
            _seed = seed;
            _screen.SetSeed(TestSeedStore.Format(seed));
            _screen.SetPlayable(true);
        }

        #region Screen events

        private void OnSeedEdited(string text)
        {
            _seed = TestSeedStore.TryParse(text, out ulong seed) ? seed : (ulong?)null;
            _screen.SetPlayable(_seed.HasValue);
        }

        private void OnRandomizeRequested()
        {
            SetSeed(_context.TestSeeds.CreateNew());
        }

        private void OnPlayRequested()
        {
            if (!_seed.HasValue)
                return;

            _context.TestSeeds.Save(_seed.Value);
            var setup = new GameSetup(_context.DefaultGameSetup.Config, _seed.Value);
            _stateMachine.ChangeState(new GameState(_context, _stateMachine, setup));
        }

        private void OnBackRequested()
        {
            _stateMachine.ChangeState(new MenuState(_context, _stateMachine));
        }

        #endregion
    }
}
