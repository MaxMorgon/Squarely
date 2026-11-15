using System;
using Squarely.Presentation;

namespace Squarely.App
{
    /// <summary>
    /// Owns one "Are you sure?" dialog for a state: creates it, closes it first when the player answers,
    /// then runs the answer. A state keeps one prompt and calls <see cref="Close"/> in its <c>Exit</c>.
    /// </summary>
    public sealed class ConfirmPrompt
    {
        private readonly ScreenFactory _screens;
        private readonly ConfirmTexts _texts;

        private ConfirmDialog _dialog;
        private Action _onConfirm;
        private Action _onCancel;

        /// <summary>Whether the dialog is open.</summary>
        public bool IsOpen => _dialog != null;

        public ConfirmPrompt(ScreenFactory screens, ConfirmTexts texts)
        {
            _screens = screens;
            _texts = texts;
        }

        /// <summary>
        /// Opens the dialog. Does nothing if it is already open.
        /// The dialog closes before either callback runs.
        /// </summary>
        /// <param name="question">The question to show. Null shows the default question.</param>
        /// <param name="onConfirm">Runs when the player says yes.</param>
        /// <param name="onCancel">Runs when the player says no. May be null.</param>
        public void Show(string question, Action onConfirm, Action onCancel = null)
        {
            if (_dialog != null)
                return;

            _onConfirm = onConfirm;
            _onCancel = onCancel;
            _dialog = _screens.CreatePanel(_screens.Prefabs.Confirm);

            if (question == null)
                _dialog.Initialize(_texts);
            else
                _dialog.Initialize(_texts, question);

            _dialog.ConfirmRequestEvent += OnConfirmRequested;
            _dialog.CancelRequestEvent += OnCancelRequested;
        }

        /// <summary>Closes the dialog without running a callback. Does nothing if it is closed.</summary>
        public void Close()
        {
            if (_dialog == null)
                return;

            _dialog.ConfirmRequestEvent -= OnConfirmRequested;
            _dialog.CancelRequestEvent -= OnCancelRequested;
            _screens.DestroyPanel(_dialog);
            _dialog = null;
            _onConfirm = null;
            _onCancel = null;
        }

        private void OnConfirmRequested()
        {
            Action action = _onConfirm;
            Close();
            action?.Invoke();
        }

        private void OnCancelRequested()
        {
            Action action = _onCancel;
            Close();
            action?.Invoke();
        }
    }
}
