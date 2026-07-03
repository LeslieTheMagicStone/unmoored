using System.Collections.Generic;
using UnityEngine;

namespace CGJ2026
{
    [RequireComponent(typeof(LineRenderer))]
    public class RopeSystem : MonoBehaviour
    {
        [SerializeField] PlayerController playerA;
        [SerializeField] PlayerController playerB;
        [SerializeField] LayerMask wrapLayer;
        [SerializeField] float wrapPointOffset = 0.05f;
        [SerializeField] int maxWrapPoints = 8;
        [SerializeField] float tautTolerance = 0.15f;

        LineRenderer lineRenderer;
        ContactFilter2D wrapFilter;
        readonly List<Vector2> wrapPoints = new();
        readonly List<Vector2> segments = new();
        readonly RaycastHit2D[] hits = new RaycastHit2D[8];
        Rigidbody2D bodyA;
        Rigidbody2D bodyB;

        public PlayerController PlayerA => playerA;
        public PlayerController PlayerB => playerB;
        public float MaxLength { get; private set; }

        void Awake()
        {
            lineRenderer = GetComponent<LineRenderer>();
            if (playerA == null || playerB == null)
                throw new System.Exception("RopeSystem: both players must be assigned");

            bodyA = playerA.GetComponent<Rigidbody2D>();
            bodyB = playerB.GetComponent<Rigidbody2D>();
            MaxLength = playerA.Config.MaxRopeLength;

            wrapFilter = new ContactFilter2D();
            wrapFilter.SetLayerMask(wrapLayer);
            wrapFilter.useLayerMask = true;
            wrapFilter.useTriggers = false;

            Physics2D.IgnoreCollision(
                playerA.GetComponent<Collider2D>(),
                playerB.GetComponent<Collider2D>(),
                true);
        }

        void Update()
        {
            UpdateWrapPoints();
            BuildSegments();
            DrawRope();
        }

        void FixedUpdate()
        {
            BuildSegments();
            EnforceConstraint();
        }

        public bool IsTaut()
        {
            BuildSegments();
            return RopeConstraintSolver.IsTaut(segments, MaxLength, tautTolerance);
        }

        public bool TryApplySwingInput(PlayerController player, float horizontalInput)
        {
            if (player == null)
                throw new System.ArgumentNullException(nameof(player));
            if (player != playerA && player != playerB)
                throw new System.ArgumentException("Player is not connected to this rope.", nameof(player));
            if (Mathf.Approximately(horizontalInput, 0f)) return false;

            BuildSegments();
            if (!RopeConstraintSolver.IsTaut(segments, MaxLength, tautTolerance)) return false;

            bool isPlayerA = player == playerA;
            Rigidbody2D playerBody = isPlayerA ? bodyA : bodyB;
            if (!CanConstraintMove(player, playerBody)) return false;

            Vector2 tangent = RopeConstraintSolver.SwingTangentForInput(
                segments,
                isPlayerA,
                horizontalInput);
            if (tangent == Vector2.zero) return false;

            float inputStrength = Mathf.Abs(horizontalInput);
            Vector2 force = player.Config.SwingForce * inputStrength * tangent;
            playerBody.AddForce(force, ForceMode2D.Force);

            PlayerController otherPlayer = isPlayerA ? playerB : playerA;
            Rigidbody2D otherBody = isPlayerA ? bodyB : bodyA;
            if (CanConstraintMove(otherPlayer, otherBody))
            {
                Vector2 reaction = -force.normalized * player.Config.SwingReactiveForce * inputStrength;
                otherBody.AddForce(reaction, ForceMode2D.Force);
            }

            return true;
        }

        void EnforceConstraint()
        {
            var stateA = new RopeEndpointState(
                bodyA.position,
                bodyA.velocity,
                ConstraintInverseMass(playerA, bodyA));
            var stateB = new RopeEndpointState(
                bodyB.position,
                bodyB.velocity,
                ConstraintInverseMass(playerB, bodyB));

            RopeConstraintResult result = RopeConstraintSolver.SolveEndpoints(
                segments,
                MaxLength,
                stateA,
                stateB);

            if (!result.AppliedCorrection) return;

            ApplyEndpointSolve(playerA, bodyA, result.Start);
            ApplyEndpointSolve(playerB, bodyB, result.End);
        }

        float ConstraintInverseMass(PlayerController player, Rigidbody2D body)
        {
            if (!CanConstraintMove(player, body)) return 0f;
            return body.mass <= 0f ? 0f : 1f / body.mass;
        }

        bool CanConstraintMove(PlayerController player, Rigidbody2D body)
        {
            return player.Form == CharacterForm.Human && body.bodyType == RigidbodyType2D.Dynamic;
        }

        void ApplyEndpointSolve(PlayerController player, Rigidbody2D body, RopeEndpointState solved)
        {
            if (!CanConstraintMove(player, body)) return;

            body.MovePosition(solved.Position);
            body.velocity = solved.Velocity;
        }

        void UpdateWrapPoints()
        {
            RemoveStaleWraps();
            for (int i = 0; i < maxWrapPoints; i++)
            {
                BuildSegments();
                if (!TryAddWrap()) return;
            }
        }

        bool TryAddWrap()
        {
            if (wrapPoints.Count >= maxWrapPoints) return false;
            for (int i = 0; i < segments.Count - 1; i++)
            {
                if (!LinecastBlocked(segments[i], segments[i + 1], out var hit)) continue;
                Vector2 wp = hit.point + hit.normal * wrapPointOffset;
                if (IsDup(wp)) continue;
                wrapPoints.Insert(i, wp);
                return true;
            }
            return false;
        }

        void RemoveStaleWraps()
        {
            for (int i = wrapPoints.Count - 1; i >= 0; i--)
            {
                Vector2 prev = i == 0 ? playerA.transform.position : wrapPoints[i - 1];
                Vector2 next = i == wrapPoints.Count - 1 ? playerB.transform.position : wrapPoints[i + 1];
                if (!LinecastBlocked(prev, next, out _))
                    wrapPoints.RemoveAt(i);
            }
        }

        bool LinecastBlocked(Vector2 a, Vector2 b, out RaycastHit2D result)
        {
            result = default;
            int n = Physics2D.Linecast(a, b, wrapFilter, hits);
            float best = float.MaxValue;
            for (int i = 0; i < n; i++)
            {
                if (hits[i].collider == null) continue;
                if (Vector2.Distance(hits[i].point, a) <= wrapPointOffset) continue;
                if (Vector2.Distance(hits[i].point, b) <= wrapPointOffset) continue;
                if (hits[i].fraction < best) { best = hits[i].fraction; result = hits[i]; }
            }
            return result.collider != null;
        }

        bool IsDup(Vector2 p)
        {
            foreach (var w in wrapPoints)
                if (Vector2.Distance(w, p) <= wrapPointOffset * 2f) return true;
            return false;
        }

        void BuildSegments()
        {
            segments.Clear();
            segments.Add(playerA.transform.position);
            segments.AddRange(wrapPoints);
            segments.Add(playerB.transform.position);
        }

        void DrawRope()
        {
            lineRenderer.positionCount = segments.Count;
            for (int i = 0; i < segments.Count; i++)
                lineRenderer.SetPosition(i, segments[i]);
        }
    }
}
