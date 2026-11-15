using System;
using NUnit.Framework;

namespace Squarely.Presentation.Tests
{
    public class DragSwingTests
    {
        private const float Frame30 = 1f / 30f;
        private const float Frame120 = 1f / 120f;

        private static DragSwing Create(float maxAngle = 12f, float smoothing = 0f, float scale = 1f)
        {
            return new DragSwing(maxAngle, 1f, 3f, 0.65f, smoothing, scale);
        }

        private static void Run(DragSwing swing, float speed, float seconds, float frame)
        {
            int count = (int)(seconds / frame + 0.5f);

            for (int i = 0; i < count; i++)
                swing.Step(speed * frame, frame);
        }

        [Test]
        public void Step_MovingRight_TiltsPositive()
        {
            DragSwing swing = Create();
            Run(swing, 5f, 1f, Frame120);
            Assert.That(swing.Angle, Is.GreaterThan(0f));
        }

        [Test]
        public void Step_MovingLeft_TiltsNegative()
        {
            DragSwing swing = Create();
            Run(swing, -5f, 1f, Frame120);
            Assert.That(swing.Angle, Is.LessThan(0f));
        }

        [Test]
        public void Step_PointerStops_SettlesToLevel()
        {
            DragSwing swing = Create(smoothing: 0.05f);
            Run(swing, 8f, 1f, Frame120);
            Run(swing, 0f, 3f, Frame120);
            Assert.That(swing.Angle, Is.EqualTo(0f).Within(0.01f));
        }

        [Test]
        public void Step_VeryFastPointer_StaysWithinMaxAngle()
        {
            DragSwing swing = Create(maxAngle: 10f);
            float largest = 0f;

            for (int i = 0; i < 600; i++)
            {
                swing.Step(50f * Frame120, Frame120);
                largest = Math.Max(largest, Math.Abs(swing.Angle));
            }

            Assert.That(largest, Is.LessThanOrEqualTo(10f));
        }

        [Test]
        public void Step_ConstantSpeed_OvershootIsSmall()
        {
            DragSwing swing = Create(maxAngle: 45f);
            float target = 8f;
            float peak = 0f;

            for (int i = 0; i < 480; i++)
            {
                swing.Step(target * Frame120, Frame120);
                peak = Math.Max(peak, swing.Angle);
            }

            Assert.That(peak, Is.LessThan(target * 1.1f));
            Assert.That(swing.Angle, Is.EqualTo(target).Within(0.05f));
        }

        [Test]
        public void Step_DifferentFrameRates_GiveSimilarAngles()
        {
            DragSwing slow = Create(smoothing: 0.05f);
            DragSwing fast = Create(smoothing: 0.05f);

            Run(slow, 6f, 0.4f, Frame30);
            Run(fast, 6f, 0.4f, Frame120);

            Assert.That(slow.Angle, Is.EqualTo(fast.Angle).Within(0.3f));
        }

        [Test]
        public void Step_HugeFrameTime_StaysFinite()
        {
            DragSwing swing = Create();
            swing.Step(100f, 5f);
            Assert.That(float.IsNaN(swing.Angle), Is.False);
            Assert.That(Math.Abs(swing.Angle), Is.LessThanOrEqualTo(12f));
        }

        [Test]
        public void Reset_ClearsTilt()
        {
            DragSwing swing = Create();
            Run(swing, 8f, 0.5f, Frame120);
            swing.Reset();
            Assert.That(swing.Angle, Is.EqualTo(0f));
            swing.Step(0f, Frame120);
            Assert.That(swing.Angle, Is.EqualTo(0f));
        }

        [Test]
        public void Angle_ScaleHalf_HalvesTheTilt()
        {
            DragSwing full = Create();
            DragSwing half = Create(scale: 0.5f);

            Run(full, 5f, 1f, Frame120);
            Run(half, 5f, 1f, Frame120);

            Assert.That(half.Angle, Is.EqualTo(full.Angle * 0.5f).Within(0.0001f));
        }

        [Test]
        public void Angle_ScaleMinusOne_FlipsTheDirection()
        {
            DragSwing swing = Create(scale: -1f);
            Run(swing, 5f, 1f, Frame120);
            Assert.That(swing.Angle, Is.LessThan(0f));
        }

        [Test]
        public void Angle_ScaleZero_StaysLevel()
        {
            DragSwing swing = Create(scale: 0f);
            Run(swing, 8f, 1f, Frame120);
            Assert.That(swing.Angle, Is.EqualTo(0f));
        }

        [Test]
        public void Angle_ScaleTwo_AppliesAfterTheMaxAngle()
        {
            DragSwing swing = Create(maxAngle: 10f, scale: 2f);
            Run(swing, 50f, 1f, Frame120);
            Assert.That(swing.Angle, Is.EqualTo(20f).Within(0.01f));
        }
    }
}
