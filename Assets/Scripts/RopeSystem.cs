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

        LineRenderer lineRenderer;

        void Awake()
        {
            lineRenderer = GetComponent<LineRenderer>();
            lineRenderer.positionCount = 2;

            if (playerA == null || playerB == null)
                throw new System.Exception("RopeSystem: both players must be assigned");
        }

        void Update()
        {
            if (playerA == null || playerB == null) return;
            lineRenderer.SetPosition(0, playerA.transform.position);
            lineRenderer.SetPosition(1, playerB.transform.position);
        }

        void FixedUpdate()
        {
            if (playerA == null || playerB == null) return;

            Vector2 posA = playerA.transform.position;
            Vector2 posB = playerB.transform.position;
            Vector2 delta = posB - posA;
            float distance = delta.magnitude;

            if (distance <= maxLength || distance < 0.001f) return;

            Vector2 dir = delta / distance;
            float excess = distance - maxLength;

            bool aAnchor = playerA.Form == CharacterForm.Anchor;
            bool bAnchor = playerB.Form == CharacterForm.Anchor;

            Rigidbody2D rbA = playerA.GetComponent<Rigidbody2D>();
            Rigidbody2D rbB = playerB.GetComponent<Rigidbody2D>();

            if (aAnchor != bAnchor)
            {
                // Pendulum: one anchored, one free
                Vector2 anchorPos = aAnchor ? posA : posB;
                Rigidbody2D freeRb = aAnchor ? rbB : rbA;

                Vector2 toFree = freeRb.position - anchorPos;
                float dist = toFree.magnitude;

                // Radial direction (from anchor to free)
                Vector2 radial = toFree / dist;
                // Tangent direction (perpendicular to radial)
                Vector2 tangent = new(-radial.y, radial.x);

                // Decompose velocity into radial and tangential
                float radialSpeed = Vector2.Dot(freeRb.velocity, radial);
                float tangentSpeed = Vector2.Dot(freeRb.velocity, tangent);

                // Cancel radial component to enforce rope constraint
                // Kill inbound velocity slightly harder to prevent bounce
                float radialDamping = radialSpeed > 0 ? 0f : 0.2f;
                radialSpeed *= radialDamping;

                // Preserve tangential component (pendulum swing), apply light drag
                tangentSpeed *= 1f - swingDamping;

                // Clamp position to maxLength
                freeRb.position = anchorPos + radial * maxLength;

                // Reconstruct velocity from tangent + remaining radial
                freeRb.velocity = radial * radialSpeed + tangent * tangentSpeed;
            }
            else
            {
                // Both human: split the slack, apply mutual pull
                float half = excess * 0.5f;
                rbA.position = posA + dir * half;
                rbB.position = posB - dir * half;

                float damp = 0.3f;
                Vector2 aRadial = Vector2.Dot(rbA.velocity, -dir) * -dir;
                Vector2 aTangent = rbA.velocity - aRadial;
                rbA.velocity = aTangent + aRadial * damp;

                Vector2 bRadial = Vector2.Dot(rbB.velocity, dir) * dir;
                Vector2 bTangent = rbB.velocity - bRadial;
                rbB.velocity = bTangent + bRadial * damp;
            }
        }
    }
}
