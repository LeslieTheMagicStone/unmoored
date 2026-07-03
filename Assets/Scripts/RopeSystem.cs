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

        LineRenderer lineRenderer;
        ContactFilter2D wrapFilter;
        readonly List<Vector2> wrapPoints = new();
        readonly List<Vector2> segments = new();
        readonly RaycastHit2D[] hits = new RaycastHit2D[8];

        public PlayerController PlayerA => playerA;
        public PlayerController PlayerB => playerB;
        public float MaxLength { get; private set; }

        void Awake()
        {
            lineRenderer = GetComponent<LineRenderer>();
            if (playerA == null || playerB == null)
                throw new System.Exception("RopeSystem: both players must be assigned");

            MaxLength = playerA.Config.MaxRopeLength;

            wrapFilter = new ContactFilter2D();
            wrapFilter.SetLayerMask(wrapLayer);
            wrapFilter.useLayerMask = true;
            wrapFilter.useTriggers = false;

            Physics2D.IgnoreCollision(playerA.GetComponent<Collider2D>(),
                playerB.GetComponent<Collider2D>(), true);
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

        void EnforceConstraint()
        {
            float len = SegmentLength();
            if (len <= MaxLength || len < 0.001f) return;

            bool aAnchor = playerA.Form == CharacterForm.Anchor;
            bool bAnchor = playerB.Form == CharacterForm.Anchor;
            if (aAnchor && bAnchor) return;

            Rigidbody2D rbA = playerA.GetComponent<Rigidbody2D>();
            Rigidbody2D rbB = playerB.GetComponent<Rigidbody2D>();

            if (aAnchor)
                ConstrainToAnchor(rbB, segments.Count - 1);
            else if (bAnchor)
                ConstrainToAnchor(rbA, 0);
            else
                ConstrainMidpoint(rbA, rbB);
        }

        void ConstrainToAnchor(Rigidbody2D freeRb, int freeIndex)
        {
            float excess = SegmentLength() - MaxLength;
            if (excess <= 0.001f) return;

            int lastIdx = segments.Count - 1;
            Vector2 from = freeIndex == 0 ? segments[1] : segments[lastIdx - 1];
            Vector2 dir = ((Vector2)freeRb.position - from).normalized;

            freeRb.MovePosition((Vector2)freeRb.position - dir * excess);
            float outward = Vector2.Dot(freeRb.velocity, dir);
            if (outward > 0f) freeRb.velocity -= dir * outward;
        }

        void ConstrainMidpoint(Rigidbody2D rbA, Rigidbody2D rbB)
        {
            float excess = SegmentLength() - MaxLength;
            if (excess <= 0.001f) return;

            float half = excess * 0.5f;
            int lastIdx = segments.Count - 1;

            Vector2 dirA = ((Vector2)rbA.position - segments[1]).normalized;
            rbA.MovePosition((Vector2)rbA.position - dirA * half);
            float outA = Vector2.Dot(rbA.velocity, dirA);
            if (outA > 0f) rbA.velocity -= dirA * outA;

            Vector2 dirB = ((Vector2)rbB.position - segments[lastIdx - 1]).normalized;
            rbB.MovePosition((Vector2)rbB.position - dirB * half);
            float outB = Vector2.Dot(rbB.velocity, dirB);
            if (outB > 0f) rbB.velocity -= dirB * outB;
        }

        float SegmentLength()
        {
            float total = 0f;
            for (int i = 0; i < segments.Count - 1; i++)
                total += Vector2.Distance(segments[i], segments[i + 1]);
            return total;
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
