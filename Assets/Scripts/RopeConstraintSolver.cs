using System.Collections.Generic;
using UnityEngine;

namespace CGJ2026
{
    public struct RopeEndpointState
    {
        public Vector2 Position;
        public Vector2 Velocity;
        public float InverseMass;

        public RopeEndpointState(Vector2 position, Vector2 velocity, float inverseMass)
        {
            Position = position;
            Velocity = velocity;
            InverseMass = Mathf.Max(0f, inverseMass);
        }
    }

    public struct RopeConstraintResult
    {
        public RopeEndpointState Start;
        public RopeEndpointState End;
        public float LengthBeforeSolve;
        public bool AppliedCorrection;

        public RopeConstraintResult(
            RopeEndpointState start,
            RopeEndpointState end,
            float lengthBeforeSolve,
            bool appliedCorrection)
        {
            Start = start;
            End = end;
            LengthBeforeSolve = lengthBeforeSolve;
            AppliedCorrection = appliedCorrection;
        }
    }

    public static class RopeConstraintSolver
    {
        const float Epsilon = 0.0001f;

        public static RopeConstraintResult SolveEndpoints(
            IReadOnlyList<Vector2> path,
            float maxLength,
            RopeEndpointState start,
            RopeEndpointState end)
        {
            ValidatePath(path);

            float length = PathLength(path);
            float excess = length - maxLength;
            float inverseMassSum = start.InverseMass + end.InverseMass;
            if (inverseMassSum <= Epsilon)
                return new RopeConstraintResult(start, end, length, false);

            Vector2 startOutward = EndpointOutwardDirection(path, true);
            Vector2 endOutward = EndpointOutwardDirection(path, false);
            bool corrected = false;

            if (excess > Epsilon)
            {
                float positionCorrection = excess / inverseMassSum;
                start.Position -= startOutward * positionCorrection * start.InverseMass;
                end.Position -= endOutward * positionCorrection * end.InverseMass;
                corrected = true;
            }

            if (length >= maxLength - Epsilon)
            {
                float outwardSpeed =
                    Vector2.Dot(start.Velocity, startOutward) +
                    Vector2.Dot(end.Velocity, endOutward);

                if (outwardSpeed > 0f)
                {
                    float velocityCorrection = outwardSpeed / inverseMassSum;
                    start.Velocity -= startOutward * velocityCorrection * start.InverseMass;
                    end.Velocity -= endOutward * velocityCorrection * end.InverseMass;
                    corrected = true;
                }
            }

            return new RopeConstraintResult(start, end, length, corrected);
        }

        public static bool IsTaut(IReadOnlyList<Vector2> path, float maxLength, float tolerance)
        {
            ValidatePath(path);
            return PathLength(path) >= maxLength - tolerance;
        }

        public static Vector2 SwingTangentForInput(
            IReadOnlyList<Vector2> path,
            bool startEndpoint,
            float horizontalInput)
        {
            ValidatePath(path);
            if (Mathf.Approximately(horizontalInput, 0f)) return Vector2.zero;

            Vector2 outward = EndpointOutwardDirection(path, startEndpoint);
            Vector2 tangent = new(-outward.y, outward.x);
            if (Vector2.Dot(tangent, Vector2.right) * horizontalInput < 0f)
                tangent = -tangent;

            return tangent;
        }

        public static Vector2 EndpointOutwardDirection(IReadOnlyList<Vector2> path, bool startEndpoint)
        {
            ValidatePath(path);

            int endpointIndex = startEndpoint ? 0 : path.Count - 1;
            int neighborIndex = startEndpoint ? 1 : path.Count - 2;
            Vector2 direction = path[endpointIndex] - path[neighborIndex];

            return direction.sqrMagnitude <= Epsilon ? Vector2.zero : direction.normalized;
        }

        public static float PathLength(IReadOnlyList<Vector2> path)
        {
            ValidatePath(path);

            float length = 0f;
            for (int i = 0; i < path.Count - 1; i++)
                length += Vector2.Distance(path[i], path[i + 1]);

            return length;
        }

        static void ValidatePath(IReadOnlyList<Vector2> path)
        {
            if (path == null)
                throw new System.ArgumentNullException(nameof(path));
            if (path.Count < 2)
                throw new System.ArgumentException("Rope path must contain both endpoints.", nameof(path));
        }
    }
}
