using System;
using NUnit.Framework;
using UnityEngine;

namespace Squarely.Presentation.Tests
{
    public class CameraFitTests
    {
        private const float Tolerance = 0.0001f;

        // 10 wide, 12 tall, centered on (1, -2).
        private static readonly Rect Area = new Rect(-4f, -8f, 10f, 12f);

        [Test]
        public void Fit_NarrowPortrait_FitsTheWidth()
        {
            CameraFrame frame = CameraFit.Fit(Area, 0.5f, 0f, 0f);

            // 10 wide at aspect 0.5 needs a view 20 tall.
            Assert.That(frame.OrthographicSize, Is.EqualTo(10f).Within(Tolerance));
            AssertNear(frame.Center, new Vector2(1f, -2f));
        }

        [Test]
        public void Fit_Landscape_FitsTheHeight()
        {
            CameraFrame frame = CameraFit.Fit(Area, 16f / 9f, 0f, 0f);

            Assert.That(frame.OrthographicSize, Is.EqualTo(6f).Within(Tolerance));
            AssertNear(frame.Center, new Vector2(1f, -2f));
        }

        [Test]
        public void Fit_Margin_AddsToEverySide()
        {
            CameraFrame portrait = CameraFit.Fit(Area, 0.5f, 1f, 0f);
            CameraFrame landscape = CameraFit.Fit(Area, 16f / 9f, 1f, 0f);

            Assert.That(portrait.OrthographicSize, Is.EqualTo(12f).Within(Tolerance));
            Assert.That(landscape.OrthographicSize, Is.EqualTo(7f).Within(Tolerance));
            AssertNear(landscape.Center, new Vector2(1f, -2f));
        }

        [Test]
        public void Fit_TopBand_KeepsTheAreaBelowIt()
        {
            const float topBand = 0.25f;
            CameraFrame frame = CameraFit.Fit(Area, 16f / 9f, 0f, topBand);

            // 12 tall in the lower 75% of the view needs a view 16 tall.
            Assert.That(frame.OrthographicSize, Is.EqualTo(8f).Within(Tolerance));

            float viewTop = frame.Center.y + frame.OrthographicSize;
            float viewBottom = frame.Center.y - frame.OrthographicSize;
            float bandBottom = viewTop - 2f * frame.OrthographicSize * topBand;
            Assert.That(Area.yMax, Is.EqualTo(bandBottom).Within(Tolerance));
            Assert.That(Area.yMin, Is.EqualTo(viewBottom).Within(Tolerance));
        }

        [Test]
        public void Fit_TopBand_WithRoomToSpare_CentersTheAreaBelowTheBand()
        {
            const float topBand = 0.25f;
            CameraFrame frame = CameraFit.Fit(Area, 0.5f, 0f, topBand);

            // The width still decides: a view 20 tall leaves 15 below the band for an area 12 tall.
            Assert.That(frame.OrthographicSize, Is.EqualTo(10f).Within(Tolerance));

            float bandBottom = frame.Center.y + frame.OrthographicSize - 2f * frame.OrthographicSize * topBand;
            float viewBottom = frame.Center.y - frame.OrthographicSize;
            Assert.That(bandBottom - Area.yMax, Is.EqualTo(Area.yMin - viewBottom).Within(Tolerance));
            Assert.That(bandBottom - Area.yMax, Is.EqualTo(1.5f).Within(Tolerance));
        }

        [Test]
        public void Fit_InvalidArguments_Throw()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => CameraFit.Fit(Area, 0f, 0f, 0f));
            Assert.Throws<ArgumentOutOfRangeException>(() => CameraFit.Fit(Area, 1f, 0f, 1f));
            Assert.Throws<ArgumentOutOfRangeException>(() => CameraFit.Fit(Area, 1f, 0f, -0.1f));
        }

        private static void AssertNear(Vector2 actual, Vector2 expected)
        {
            Assert.That(actual.x, Is.EqualTo(expected.x).Within(Tolerance), "x");
            Assert.That(actual.y, Is.EqualTo(expected.y).Within(Tolerance), "y");
        }
    }
}
