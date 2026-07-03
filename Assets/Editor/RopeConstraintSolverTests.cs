using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace CGJ2026.Tests
{
    public class RopeConstraintSolverTests
    {
        [Test]
        public void AnchoredEndpointKeepsFreeEndpointWithinMaxLength()
        {
            var path = new List<Vector2>
            {
                Vector2.zero,
                new(0f, -8f)
            };
            var anchor = new RopeEndpointState(Vector2.zero, Vector2.zero, 0f);
            var free = new RopeEndpointState(new Vector2(0f, -8f), Vector2.zero, 1f);

            RopeConstraintResult result = RopeConstraintSolver.SolveEndpoints(path, 6f, anchor, free);

            Assert.That(result.AppliedCorrection, Is.True);
            Assert.That(result.Start.Position, Is.EqualTo(Vector2.zero));
            Assert.That(result.End.Position.y, Is.EqualTo(-6f).Within(0.001f));
        }

        [Test]
        public void FallingPlayerTransfersOutwardVelocityToOtherMovableEndpoint()
        {
            var path = new List<Vector2>
            {
                Vector2.zero,
                new(0f, -6.5f)
            };
            var upper = new RopeEndpointState(Vector2.zero, Vector2.zero, 1f);
            var lower = new RopeEndpointState(new Vector2(0f, -6.5f), new Vector2(0f, -10f), 1f);

            RopeConstraintResult result = RopeConstraintSolver.SolveEndpoints(path, 6f, upper, lower);

            Assert.That(result.Start.Velocity.y, Is.LessThan(-4.9f));
            Assert.That(result.End.Velocity.y, Is.GreaterThan(-10f));
            Assert.That(result.End.Velocity.y, Is.EqualTo(-5f).Within(0.001f));
        }

        [Test]
        public void TautRopeClipsOutwardVelocityBeforeItStretches()
        {
            var path = new List<Vector2>
            {
                Vector2.zero,
                new(0f, -6f)
            };
            var upper = new RopeEndpointState(Vector2.zero, Vector2.zero, 1f);
            var lower = new RopeEndpointState(new Vector2(0f, -6f), new Vector2(0f, -10f), 1f);

            RopeConstraintResult result = RopeConstraintSolver.SolveEndpoints(path, 6f, upper, lower);

            Assert.That(result.AppliedCorrection, Is.True);
            Assert.That(result.Start.Position, Is.EqualTo(Vector2.zero));
            Assert.That(result.End.Position.y, Is.EqualTo(-6f).Within(0.001f));
            Assert.That(result.Start.Velocity.y, Is.EqualTo(-5f).Within(0.001f));
            Assert.That(result.End.Velocity.y, Is.EqualTo(-5f).Within(0.001f));
        }

        [Test]
        public void SwingTangentUsesEndpointSegmentAfterRopeWraps()
        {
            var path = new List<Vector2>
            {
                Vector2.zero,
                new(1f, 0f),
                new(1f, -6f)
            };

            Vector2 tangent = RopeConstraintSolver.SwingTangentForInput(path, false, 1f);

            Assert.That(tangent.x, Is.GreaterThan(0.99f));
            Assert.That(Mathf.Abs(tangent.y), Is.LessThan(0.001f));
        }
    }
}
