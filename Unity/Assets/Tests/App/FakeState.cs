using System;
using System.Collections.Generic;

namespace Squarely.App.Tests
{
    /// <summary>
    /// Test state that records its Enter and Exit calls into a shared log and can run an action on either.
    /// </summary>
    internal sealed class FakeState : ITickableState
    {
        private readonly string _name;
        private readonly List<string> _log;

        public Action OnEnter { get; set; }

        public Action OnExit { get; set; }

        public FakeState(string name, List<string> log)
        {
            _name = name;
            _log = log;
        }

        public void Enter()
        {
            _log.Add($"{_name}.Enter");
            OnEnter?.Invoke();
        }

        public void Tick(float deltaTime)
        {
            _log.Add($"{_name}.Tick");
        }

        public void Exit()
        {
            _log.Add($"{_name}.Exit");
            OnExit?.Invoke();
        }
    }
}
