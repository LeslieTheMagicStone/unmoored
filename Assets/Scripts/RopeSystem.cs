using System.Collections.Generic;
using UnityEngine;

namespace CGJ2026
{
    [RequireComponent(typeof(LineRenderer))]
    public class RopeSystem : MonoBehaviour
    {
        [SerializeField] PlayerController playerA;
        [SerializeField] PlayerController playerB;
        [SerializeField] float maxLength = 6f;
        [SerializeField] [Range(0f, 1f)] float swingDamping = 0.1f;
        [SerializeField] LayerMask wrapLayer = Physics2D.DefaultRaycastLayers;
        [SerializeField] float wrapPointOffset = 0.05f;
        [SerializeField] int maxWrapPoints = 8;
        [SerializeField] float ropeStiffness = 80f;
        [SerializeField] float maxPullForce = 120f;
        [SerializeField] [Range(0f, 1f)] float radialVelocityDamping = 0.25f;
        [SerializeField] float slackLength = 0.25f;
        [SerializeField] float retractSpeed = 3f;
        [SerializeField] float payoutSpeed = 12f;
        [SerializeField] float sagDepthPerSlack = 0.5f;
        [SerializeField] float maxVisualSag = 0.8f;
        [SerializeField] int sagSegmentsPerSegment = 6;

        LineRenderer lineRenderer;
        ContactFilter2D wrapFilter;
        float currentLength;
        readonly List<Vector2> wrapPoints = new();
        readonly List<Vector2> ropePoints = new();
        readonly List<Vector2> renderPoints = new();
        readonly RaycastHit2D[] raycastHits = new RaycastHit2D[8];

        void Awake()
        {
            lineRenderer = GetComponent<LineRenderer>();
            lineRenderer.positionCount = 2;

            if (playerA == null || playerB == null)
                throw new System.Exception("RopeSystem: both players must be assigned");

            currentLength = Mathf.Clamp(Vector2.Distance(playerA.transform.position, playerB.transform.position) + slackLength, 0f, maxLength);

            wrapFilter = new ContactFilter2D();
            wrapFilter.SetLayerMask(wrapLayer);
            wrapFilter.useLayerMask = true;
            wrapFilter.useTriggers = false;
        }

        void Update()
        {
            if (playerA == null || playerB == null) return;

            UpdateWrapPoints();
            BuildRopePoints();
            UpdateLineRenderer();
        }

        void FixedUpdate()
        {
            if (playerA == null || playerB == null) return;

            BuildRopePoints();

            float ropeLength = GetRopeLength();
            UpdateCurrentLength(ropeLength, Time.fixedDeltaTime);

            if (ropeLength <= currentLength || ropeLength < 0.001f) return;

            float excess = ropeLength - currentLength;
            Vector2 pullA = GetEndpointPullDirection(0, 1);
            Vector2 pullB = GetEndpointPullDirection(ropePoints.Count - 1, ropePoints.Count - 2);

            bool aAnchor = playerA.Form == CharacterForm.Anchor;
            bool bAnchor = playerB.Form == CharacterForm.Anchor;

            Rigidbody2D rbA = playerA.GetComponent<Rigidbody2D>();
            Rigidbody2D rbB = playerB.GetComponent<Rigidbody2D>();

            if (aAnchor != bAnchor)
            {
                Rigidbody2D freeRb = aAnchor ? rbB : rbA;
                Vector2 freePull = aAnchor ? pullB : pullA;
                ApplyElasticPull(freeRb, freePull, excess);
                DampOutwardVelocity(freeRb, freePull, radialVelocityDamping);
            }
            else
            {
                ApplyElasticPull(rbA, pullA, excess * 0.5f);
                ApplyElasticPull(rbB, pullB, excess * 0.5f);
                DampOutwardVelocity(rbA, pullA, radialVelocityDamping);
                DampOutwardVelocity(rbB, pullB, radialVelocityDamping);
            }
        }

        void UpdateCurrentLength(float ropeLength, float deltaTime)
        {
            float targetLength = Mathf.Clamp(ropeLength + slackLength, 0f, maxLength);
            float speed = targetLength < currentLength ? retractSpeed : payoutSpeed;
            currentLength = Mathf.MoveTowards(currentLength, targetLength, speed * deltaTime);
        }

        void UpdateWrapPoints()
        {
            RemoveUnusedWrapPoints();

            for (int i = 0; i < maxWrapPoints; i++)
            {
                BuildRopePoints();
                if (!TryAddWrapPoint()) return;
            }
        }

        bool TryAddWrapPoint()
        {
            if (wrapPoints.Count >= maxWrapPoints) return false;

            for (int i = 0; i < ropePoints.Count - 1; i++)
            {
                if (!TryGetBlockingHit(ropePoints[i], ropePoints[i + 1], out RaycastHit2D hit))
                    continue;

                Vector2 wrapPoint = GetWrapPoint(hit, ropePoints[i], ropePoints[i + 1]);
                if (IsDuplicateWrapPoint(wrapPoint)) continue;

                wrapPoints.Insert(i, wrapPoint);
                return true;
            }

            return false;
        }

        void RemoveUnusedWrapPoints()
        {
            for (int i = wrapPoints.Count - 1; i >= 0; i--)
            {
                Vector2 previous = i == 0 ? playerA.transform.position : wrapPoints[i - 1];
                Vector2 next = i == wrapPoints.Count - 1 ? playerB.transform.position : wrapPoints[i + 1];

                if (!TryGetBlockingHit(previous, next, out _))
                    wrapPoints.RemoveAt(i);
            }
        }

        Vector2 GetWrapPoint(RaycastHit2D hit, Vector2 segmentStart, Vector2 segmentEnd)
        {
            Bounds bounds = hit.collider.bounds;
            Vector2 center = bounds.center;
            Vector2[] candidates =
            {
                new(bounds.min.x, bounds.min.y),
                new(bounds.min.x, bounds.max.y),
                new(bounds.max.x, bounds.min.y),
                new(bounds.max.x, bounds.max.y)
            };

            Vector2 bestPoint = hit.point + hit.normal * wrapPointOffset;
            float bestScore = float.MaxValue;

            for (int i = 0; i < candidates.Length; i++)
            {
                Vector2 directionFromCenter = (candidates[i] - center).normalized;
                Vector2 candidate = candidates[i] + directionFromCenter * wrapPointOffset;
                float score = Vector2.Distance(hit.point, candidate);

                if (TryGetBlockingHit(segmentStart, candidate, out _))
                    score += 100f;
                if (TryGetBlockingHit(candidate, segmentEnd, out _))
                    score += 100f;

                if (score < bestScore)
                {
                    bestScore = score;
                    bestPoint = candidate;
                }
            }

            return bestPoint;
        }

        bool TryGetBlockingHit(Vector2 start, Vector2 end, out RaycastHit2D blockingHit)
        {
            blockingHit = default;
            int hitCount = Physics2D.Linecast(start, end, wrapFilter, raycastHits);
            float closestFraction = float.MaxValue;

            for (int i = 0; i < hitCount; i++)
            {
                RaycastHit2D hit = raycastHits[i];
                if (hit.collider == null) continue;
                if (Vector2.Distance(hit.point, start) <= wrapPointOffset) continue;
                if (Vector2.Distance(hit.point, end) <= wrapPointOffset) continue;

                if (hit.fraction < closestFraction)
                {
                    closestFraction = hit.fraction;
                    blockingHit = hit;
                }
            }

            return blockingHit.collider != null;
        }

        bool IsDuplicateWrapPoint(Vector2 point)
        {
            for (int i = 0; i < wrapPoints.Count; i++)
                if (Vector2.Distance(wrapPoints[i], point) <= wrapPointOffset * 2f)
                    return true;

            return false;
        }

        void BuildRopePoints()
        {
            ropePoints.Clear();
            ropePoints.Add(playerA.transform.position);

            for (int i = 0; i < wrapPoints.Count; i++)
                ropePoints.Add(wrapPoints[i]);

            ropePoints.Add(playerB.transform.position);
        }

        float GetRopeLength()
        {
            float length = 0f;

            for (int i = 0; i < ropePoints.Count - 1; i++)
                length += Vector2.Distance(ropePoints[i], ropePoints[i + 1]);

            return length;
        }

        void UpdateLineRenderer()
        {
            renderPoints.Clear();

            float ropeLength = GetRopeLength();
            float visualSlack = Mathf.Max(0f, currentLength - ropeLength);

            if (visualSlack <= 0.001f || sagSegmentsPerSegment <= 1 || ropeLength <= 0.001f)
            {
                for (int i = 0; i < ropePoints.Count; i++)
                    renderPoints.Add(ropePoints[i]);
            }
            else
            {
                BuildSaggedRenderPoints(ropeLength, visualSlack);
            }

            lineRenderer.positionCount = renderPoints.Count;

            for (int i = 0; i < renderPoints.Count; i++)
                lineRenderer.SetPosition(i, renderPoints[i]);
        }

        void BuildSaggedRenderPoints(float ropeLength, float visualSlack)
        {
            renderPoints.Add(ropePoints[0]);

            for (int i = 0; i < ropePoints.Count - 1; i++)
            {
                Vector2 start = ropePoints[i];
                Vector2 end = ropePoints[i + 1];
                float segmentLength = Vector2.Distance(start, end);
                float segmentSlack = visualSlack * segmentLength / ropeLength;
                float sagDepth = Mathf.Min(segmentSlack * sagDepthPerSlack, maxVisualSag);

                for (int step = 1; step <= sagSegmentsPerSegment; step++)
                {
                    float t = step / (float)sagSegmentsPerSegment;
                    Vector2 point = Vector2.Lerp(start, end, t);
                    point += Vector2.down * Mathf.Sin(t * Mathf.PI) * sagDepth;
                    renderPoints.Add(point);
                }
            }
        }

        Vector2 GetEndpointPullDirection(int endpointIndex, int neighborIndex)
        {
            Vector2 delta = ropePoints[neighborIndex] - ropePoints[endpointIndex];
            if (delta.sqrMagnitude < 0.0001f) return Vector2.zero;
            return delta.normalized;
        }

        void ApplyElasticPull(Rigidbody2D rb, Vector2 pullDirection, float excess)
        {
            if (pullDirection == Vector2.zero) return;

            float force = Mathf.Min(excess * ropeStiffness, maxPullForce);
            rb.AddForce(pullDirection * force, ForceMode2D.Force);
        }

        void DampOutwardVelocity(Rigidbody2D rb, Vector2 pullDirection, float damping)
        {
            if (pullDirection == Vector2.zero) return;

            Vector2 outwardDirection = -pullDirection;
            float outwardSpeed = Vector2.Dot(rb.velocity, outwardDirection);
            if (outwardSpeed <= 0f) return;

            Vector2 outwardVelocity = outwardDirection * outwardSpeed;
            Vector2 remainingVelocity = rb.velocity - outwardVelocity;
            rb.velocity = remainingVelocity + outwardVelocity * damping * (1f - swingDamping);
        }
    }
}
