using System;
using Squarely.Presentation;
using Squarely.Simulation;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Squarely.App
{
    /// <summary>
    /// Watches a recorded game on a game view, with no player input. Starts a game with the replay's seed and the
    /// default game mode, then runs the replay's commands one by one through the game controller, so the
    /// playback is the same as in a game. Auto-play runs the next command a short delay after the previous playback ended.
    /// It starts paused. Play/Pause, Step and the speed button control it. A banner warns if the replay was recorded with a different game config.
    /// If a command fails, it stops and says where. At the end it shows the final score and whether the end state matches the recording.
    /// It never records a high score or a history entry and has no game-over screen. Back enters the back target.
    /// </summary>
    public sealed class ReplayState : IAppState
    {
        private readonly AppContext _context;
        private readonly AppStateMachine _stateMachine;
        private readonly GameReplay _replay;
        private readonly Func<IAppState> _createBackState;

        private GameView _view;
        private GameController _game;
        private ReplayScreen _screen;
        private ReplayStepper _stepper;
        private int _speedIndex;
        private bool _isOver;
        private int _movesShown;

        /// <param name="createBackState">Creates the state Back enters.</param>
        public ReplayState(AppContext context, AppStateMachine stateMachine, GameReplay replay, Func<IAppState> createBackState)
        {
            _context = context;
            _stateMachine = stateMachine;
            _replay = replay ?? throw new ArgumentNullException(nameof(replay));
            _createBackState = createBackState ?? throw new ArgumentNullException(nameof(createBackState));
        }

        public void Enter()
        {
            SimConfig config = _context.DefaultConfig;
            Debug.Log($"Watching a replay, seed {_replay.Seed}, {_replay.Commands.Count} commands.");

            _view = Object.Instantiate(_context.ScreenPrefabs.Game, _context.ScreenRoot);
            _game = new GameController(_view, new GameSetup(config, _replay.Seed));
            _view.Initialize(config, _context.Camera, _context.Texts.GameHud, _game);
            _view.SetInputBlocked(true);
            _view.Hud.HideSettings();

            _screen = _context.CreateScreen(_context.ScreenPrefabs.Replay);
            _screen.Initialize(_context.Texts.Replay);
            _screen.SetWarning(ReplayPaste.Check(_replay, config).Status == ReplayPasteStatus.ConfigMismatch);

            _stepper = new ReplayStepper(_replay.Commands.Count, _screen.StepDelay);
            _speedIndex = Mathf.Max(0, Array.IndexOf(_screen.SpeedPresets, 1f));
            ApplySpeed();
            _stepper.SetPaused(true);
            _screen.SetPlaying(false);
            UpdateProgress();

            _screen.PlayPauseRequestEvent += OnPlayPauseRequested;
            _screen.StepRequestEvent += OnStepRequested;
            _screen.SpeedRequestEvent += OnSpeedRequested;
            _screen.BackRequestEvent += OnBackRequested;
            _screen.TickEvent += OnTick;

            // The opening deal plays like a game's.
            _game.StartGame();
        }

        public void Exit()
        {
            _screen.PlayPauseRequestEvent -= OnPlayPauseRequested;
            _screen.StepRequestEvent -= OnStepRequested;
            _screen.SpeedRequestEvent -= OnSpeedRequested;
            _screen.BackRequestEvent -= OnBackRequested;
            _screen.TickEvent -= OnTick;
            Object.Destroy(_screen.gameObject);
            Object.Destroy(_view.gameObject);
        }

        #region Screen events

        private void OnTick(float deltaTime)
        {
            if (_isOver)
                return;

            float scaledTime = deltaTime * _view.Player.Speed;

            if (_stepper.Tick(scaledTime, _view.Player.IsPlaying))
                ExecuteNext();

            // The result waits for the last playback, also when the last command just ran.
            if (!_isOver && !_stepper.HasNext && !_view.Player.IsPlaying)
                ShowResult();
        }

        private void OnPlayPauseRequested()
        {
            _stepper.SetPaused(!_stepper.IsPaused);
            _screen.SetPlaying(!_stepper.IsPaused);
        }

        private void OnStepRequested()
        {
            if (!_isOver && _stepper.HasNext)
                ExecuteNext();
        }

        private void OnSpeedRequested()
        {
            _speedIndex = (_speedIndex + 1) % _screen.SpeedPresets.Length;
            ApplySpeed();
        }

        private void OnBackRequested()
        {
            _stateMachine.ChangeState(_createBackState());
        }

        #endregion

        private void ExecuteNext()
        {
            int index = _stepper.Executed;
            ISimCommand command = _replay.Commands[index];

            if (!_game.ExecuteCommand(command))
            {
                _stepper.Stop();
                Finish(string.Format(_context.Texts.Replay.DivergedFormat, index + 1));
                return;
            }

            _stepper.MarkExecuted();

            if (command is PlacePieceCommand)
                _movesShown++;

            UpdateProgress(command is EndGameCommand);
        }

        private void ShowResult()
        {
            ReplayTexts texts = _context.Texts.Replay;
            Sim sim = _game.CopySim();
            bool matches = _replay.MatchesEnd(sim);
            string recordedState = _replay.EndIsResolved ? texts.ResultFinished : texts.ResultUnfinished;
            string comparison = matches ? texts.ResultMatch : string.Format(texts.ResultMismatchFormat, _replay.EndScore, recordedState);

            Finish(string.Format(texts.ResultFormat, sim.Score) + "\n" + comparison);
        }

        private void Finish(string status)
        {
            _isOver = true;
            _stepper.SetPaused(true);
            _screen.SetPlaying(false);
            _screen.SetControlsEnabled(false);
            _screen.SetStatus(status);
        }

        private void ApplySpeed()
        {
            float speed = _screen.SpeedPresets[_speedIndex];
            _view.Player.Speed = speed;
            _screen.SetSpeed(speed);
        }

        private void UpdateProgress(bool isGameEnded = false)
        {
            if (isGameEnded)
                _screen.SetGameEnded();
            else
                _screen.SetProgress(_movesShown, _replay.MoveCount);
        }
    }
}
