using System.Collections.Generic;
using NUnit.Framework;
using Squarely.Simulation;

namespace Squarely.App.Tests
{
    public sealed class ReplayPasteTests
    {
        private SimConfig _config;

        [SetUp]
        public void SetUp()
        {
            _config = DefaultSimConfig.Create();
        }

        private string Code(ulong fingerprint)
        {
            var replay = new GameReplay(7, fingerprint, new List<ISimCommand>(), 0, false);
            return ReplayCodec.Encode(replay);
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]
        public void Check_EmptyText_IsEmpty(string text)
        {
            Assert.That(ReplayPaste.Check(text, _config).Status, Is.EqualTo(ReplayPasteStatus.Empty));
        }

        [Test]
        public void Check_NotACode_IsInvalidWithError()
        {
            ReplayPasteResult result = ReplayPaste.Check("hello", _config);

            Assert.That(result.Status, Is.EqualTo(ReplayPasteStatus.Invalid));
            Assert.That(result.Error, Is.Not.Empty);
            Assert.That(result.CanPlay, Is.False);
        }

        [Test]
        public void Check_CorruptCode_IsInvalid()
        {
            string code = Code(ConfigFingerprint.Compute(_config));

            Assert.That(ReplayPaste.Check(code + "!", _config).Status, Is.EqualTo(ReplayPasteStatus.Invalid));
        }

        [Test]
        public void Check_MatchingFingerprint_IsOk()
        {
            string code = Code(ConfigFingerprint.Compute(_config));

            ReplayPasteResult result = ReplayPaste.Check(code, _config);

            Assert.That(result.Status, Is.EqualTo(ReplayPasteStatus.Ok));
            Assert.That(result.CanPlay, Is.True);
            Assert.That(result.Replay.Seed, Is.EqualTo(7UL));
        }

        [Test]
        public void Check_OtherFingerprint_IsMismatchAndCanPlay()
        {
            string code = Code(ConfigFingerprint.Compute(_config) + 1);

            ReplayPasteResult result = ReplayPaste.Check(code, _config);

            Assert.That(result.Status, Is.EqualTo(ReplayPasteStatus.ConfigMismatch));
            Assert.That(result.CanPlay, Is.True);
        }

        [Test]
        public void Check_CodeWithSurroundingSpace_IsOk()
        {
            string code = Code(ConfigFingerprint.Compute(_config));

            Assert.That(ReplayPaste.Check("  " + code + "\n", _config).Status, Is.EqualTo(ReplayPasteStatus.Ok));
        }
    }
}
