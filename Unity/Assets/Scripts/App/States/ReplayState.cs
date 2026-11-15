#if UNITY_EDITOR || SQUARELY_DEV_TOOLS
using System;
using System.Collections.Generic;
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
    /// It starts paused. Play/Pause, Jump to start, Step back, Step forward, Jump to end and the speed dropdown control it. Step back undoes the last command and Jump to start undoes all of them, at once, with no animation. Both pause auto-play and also work after the replay is over, and then let the replay run again. Jump to end runs all remaining commands at once, with no animation and no sound, and pauses auto-play. A banner warns if the replay was recorded with a different game config.
    /// If a command fails, it stops and says where. At the end it shows the final score and whether the end state matches the recording.
    /// It never records a high score or a history entry and has no game-over screen. Back enters the back target.
    /// </summary>
    public sealed class ReplayState : ITickableState
    {
        private readonly AppContext _context;
        private readonly AppStateMachine _stateMachine;
        private readonly GameReplay _replay;
        private readonly Func<IAppState> _createBackState;

        private GameView _view;
        private GameController _game;
        private ReplayScreen _screen;
        private ReplayStepper _stepper;
        private bool _isOver;
        private int _movesShown;

        // The sim before each executed command: _snapshots[i] is the state Step back to command i goes to.
        private readonly List<Sim> _snapshots = new List<Sim>();

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

            (_view, _game) = _context.Screens.CreateGame(new GameSetup(config, _replay.Seed));
            _view.SetInputBlocked(true);
            _view.Hud.HideSettings();

            _screen = _context.Screens.CreateScreen(_context.Screens.Prefabs.Replay);
            _screen.Initialize(_context.Texts.Replay);
            _screen.SetWarning(ReplayPaste.Check(_replay, config).Status == ReplayPasteStatus.ConfigMismatch);

            _stepper = new ReplayStepper(_replay.Commands.Count, _screen.StepDelay);
            _view.Player.Speed = 1f;
            _screen.SetSpeed(1f);
            _stepper.SetPaused(true);
            _screen.SetPlaying(false);
            UpdateProgress();
            UpdateBackControls();

            _screen.PlayPauseRequestEvent += OnPlayPauseRequested;
            _screen.StepBackRequestEvent += OnStepBackRequested;
            _screen.StepForwardRequestEvent += OnStepForwardRequested;
            _screen.JumpStartRequestEvent += OnJumpStartRequested;
            _screen.JumpEndRequestEvent += OnJumpEndRequested;
            _screen.SpeedSelectEvent += OnSpeedSelected;
            _screen.BackRequestEvent += OnBackRequested;

            // The opening deal plays like a game's.
            _game.StartGame();
        }

        public void Exit()
        {
            _screen.PlayPauseRequestEvent -= OnPlayPauseRequested;
            _screen.StepBackRequestEvent -= OnStepBackRequested;
            _screen.StepForwardRequestEvent -= OnStepForwardRequested;
            _screen.JumpStartRequestEvent -= OnJumpStartRequested;
            _screen.JumpEndRequestEvent -= OnJumpEndRequested;
            _screen.SpeedSelectEvent -= OnSpeedSelected;
            _screen.BackRequestEvent -= OnBackRequested;
            Object.Destroy(_screen.gameObject);
            Object.Destroy(_view.gameObject);
        }

        public void Tick(float deltaTime)
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

        #region Screen events

        private void OnPlayPauseRequested()
        {
            _stepper.SetPaused(!_stepper.IsPaused);
            _screen.SetPlaying(!_stepper.IsPaused);
        }

        private void OnStepBackRequested()
        {
            if (_stepper.CanStepBack)
                GoBackTo(_stepper.Executed - 1);
        }

        private void OnJumpStartRequested()
        {
            if (_stepper.CanStepBack)
                GoBackTo(0);
        }

        private void OnJumpEndRequested()
        {
            if (_isOver || !_stepper.HasNext)
                return;

            // Each command applies at once, so no playback runs and no sounds burst.
            while (!_isOver && _stepper.HasNext)
                ExecuteNext(isSnapped: true);

            _stepper.SetPaused(true);
            _screen.SetPlaying(false);

            if (!_isOver)
                ShowResult();
        }

        // The one path back to the state after the first count commands.
        private void GoBackTo(int count)
        {
            _stepper.StepBackTo(count);
            _game.RestoreSim(_snapshots[count], count);

            for (int i = count; i < _snapshots.Count; i++)
            {
                if (_replay.Commands[i] is PlacePieceCommand)
                    _movesShown--;
            }

            _snapshots.RemoveRange(count, _snapshots.Count - count);

            // Stepping back from the end brings the replay to life again.
            _isOver = false;
            _screen.SetStatus(null);
            _screen.SetControlsEnabled(true);
            _screen.SetPlaying(false);
            UpdateProgress();
            UpdateBackControls();
        }

        private void OnStepForwardRequested()
        {
            if (!_isOver && _stepper.HasNext)
                ExecuteNext();
        }

        private void OnSpeedSelected(float speed)
        {
            _view.Player.Speed = speed;
        }

        private void OnBackRequested()
        {
            _stateMachine.ChangeState(_createBackState());
        }

        #endregion

        private void ExecuteNext(bool isSnapped = false)
        {
            int index = _stepper.Executed;
            ISimCommand command = _replay.Commands[index];
            Sim before = _game.CopySim();

            if (!_game.ExecuteCommand(command, isSnapped))
            {
                _stepper.Stop();
                Finish(string.Format(_context.Texts.Replay.DivergedFormat, index + 1));
                return;
            }

            _snapshots.Add(before);
            _stepper.MarkExecuted();

            if (command is PlacePieceCommand)
                _movesShown++;

            UpdateProgress(command is EndGameCommand);
            UpdateBackControls();
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

        private void UpdateBackControls()
        {
            _screen.SetBackControlsEnabled(_stepper.CanStepBack);
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
#endif
