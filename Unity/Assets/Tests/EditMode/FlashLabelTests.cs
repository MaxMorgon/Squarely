using NUnit.Framework;

namespace Squarely.Presentation.Tests
{
    public class FlashLabelTests
    {
        private string _text;
        private bool _visible;

        private FlashLabel Create(float seconds = 1.5f)
        {
            return new FlashLabel(text => _text = text, visible => _visible = visible, seconds);
        }

        [Test]
        public void Create_StartsHidden()
        {
            _visible = true;
            FlashLabel label = Create();

            Assert.IsFalse(_visible);
            Assert.IsFalse(label.IsVisible);
        }

        [Test]
        public void Show_ShowsTheTextAtOnce()
        {
            FlashLabel label = Create();

            label.Flash("Copied", 10f);

            Assert.IsTrue(_visible);
            Assert.AreEqual("Copied", _text);
            Assert.IsTrue(label.IsVisible);
        }

        [Test]
        public void Tick_BeforeTheTimeIsUp_KeepsShowing()
        {
            FlashLabel label = Create(1.5f);
            label.Flash("Copied", 10f);

            label.Tick(11.4f);

            Assert.IsTrue(_visible);
        }

        [Test]
        public void Tick_WhenTheTimeIsUp_Hides()
        {
            FlashLabel label = Create(1.5f);
            label.Flash("Copied", 10f);

            label.Tick(11.5f);

            Assert.IsFalse(_visible);
            Assert.IsFalse(label.IsVisible);
        }

        [Test]
        public void Show_AgainBeforeTheTimeIsUp_RestartsTheTime()
        {
            FlashLabel label = Create(1.5f);
            label.Flash("Copied", 10f);
            label.Flash("Copied", 11f);

            label.Tick(12f);

            Assert.IsTrue(_visible);
        }
    }
}
