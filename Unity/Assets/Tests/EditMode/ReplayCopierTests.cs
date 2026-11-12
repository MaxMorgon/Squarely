using NUnit.Framework;
using Squarely.Simulation;

namespace Squarely.Presentation.Tests
{
    public sealed class ReplayCopierTests
    {
        private sealed class FakeClipboard : IClipboard
        {
            public string Text;

            public void SetText(string text)
            {
                Text = text;
            }
        }

        [Test]
        public void Copy_PutsTheReplayCodeOnTheClipboard()
        {
            var clipboard = new FakeClipboard();
            var replay = new GameReplay(7UL, 1UL, new ISimCommand[] { new EndGameCommand() }, 0, true);

            string code = new ReplayCopier(clipboard).Copy(replay);

            Assert.That(clipboard.Text, Is.EqualTo(code));
            Assert.That(clipboard.Text, Is.EqualTo(ReplayCodec.Encode(replay)));
            Assert.That(clipboard.Text, Does.StartWith("SQ1."));
        }
    }
}
