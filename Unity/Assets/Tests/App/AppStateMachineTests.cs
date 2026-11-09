using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Squarely.App.Tests
{
    public sealed class AppStateMachineTests
    {
        private List<string> _log;
        private AppStateMachine _machine;

        [SetUp]
        public void SetUp()
        {
            _log = new List<string>();
            _machine = new AppStateMachine();
        }

        [Test]
        public void Current_BeforeFirstChange_IsNull()
        {
            Assert.That(_machine.Current, Is.Null);
        }

        [Test]
        public void ChangeState_First_EntersWithoutExit()
        {
            var a = new FakeState("A", _log);

            _machine.ChangeState(a);

            Assert.That(_log, Is.EqualTo(new[] { "A.Enter" }));
            Assert.That(_machine.Current, Is.SameAs(a));
        }

        [Test]
        public void ChangeState_ExitsCurrentBeforeEnteringNext()
        {
            var a = new FakeState("A", _log);
            var b = new FakeState("B", _log);

            _machine.ChangeState(a);
            _machine.ChangeState(b);

            Assert.That(_log, Is.EqualTo(new[] { "A.Enter", "A.Exit", "B.Enter" }));
            Assert.That(_machine.Current, Is.SameAs(b));
        }

        [Test]
        public void ChangeState_ToSameState_ExitsAndReenters()
        {
            var a = new FakeState("A", _log);

            _machine.ChangeState(a);
            _machine.ChangeState(a);

            Assert.That(_log, Is.EqualTo(new[] { "A.Enter", "A.Exit", "A.Enter" }));
        }

        [Test]
        public void ChangeState_RequestedDuringEnter_RunsAfterEnterCompletes()
        {
            var a = new FakeState("A", _log);
            var b = new FakeState("B", _log);
            a.OnEnter = () =>
            {
                _machine.ChangeState(b);
                _log.Add("A.EnterDone");
            };

            _machine.ChangeState(a);

            Assert.That(_log, Is.EqualTo(new[] { "A.Enter", "A.EnterDone", "A.Exit", "B.Enter" }));
            Assert.That(_machine.Current, Is.SameAs(b));
        }

        [Test]
        public void ChangeState_RequestedDuringExit_RunsAfterCurrentTransition()
        {
            var a = new FakeState("A", _log);
            var b = new FakeState("B", _log);
            var c = new FakeState("C", _log);
            _machine.ChangeState(a);
            a.OnExit = () => _machine.ChangeState(c);

            _machine.ChangeState(b);

            Assert.That(_log, Is.EqualTo(new[] { "A.Enter", "A.Exit", "B.Enter", "B.Exit", "C.Enter" }));
            Assert.That(_machine.Current, Is.SameAs(c));
        }

        [Test]
        public void ChangeState_SeveralRequestedDuringEnter_RunInRequestOrder()
        {
            var a = new FakeState("A", _log);
            var b = new FakeState("B", _log);
            var c = new FakeState("C", _log);
            a.OnEnter = () =>
            {
                _machine.ChangeState(b);
                _machine.ChangeState(c);
            };

            _machine.ChangeState(a);

            Assert.That(_log, Is.EqualTo(new[] { "A.Enter", "A.Exit", "B.Enter", "B.Exit", "C.Enter" }));
        }

        [Test]
        public void ChangeState_Null_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => _machine.ChangeState(null));
        }

        [Test]
        public void ChangeState_AfterStateThrew_StillWorks()
        {
            var a = new FakeState("A", _log);
            var b = new FakeState("B", _log);
            a.OnEnter = () => throw new InvalidOperationException();

            Assert.Throws<InvalidOperationException>(() => _machine.ChangeState(a));
            _machine.ChangeState(b);

            Assert.That(_machine.Current, Is.SameAs(b));
            Assert.That(_log, Is.EqualTo(new[] { "A.Enter", "A.Exit", "B.Enter" }));
        }
    }
}
