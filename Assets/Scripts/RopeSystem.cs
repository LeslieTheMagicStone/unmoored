using UnityEngine;

namespace CGJ2026
{
    [RequireComponent(typeof(LineRenderer))]
    public class RopeSystem : MonoBehaviour
    {
        [SerializeField] PlayerController playerA;
        [SerializeField] PlayerController playerB;

        LineRenderer lineRenderer;

        public PlayerController PlayerA => playerA;
        public PlayerController PlayerB => playerB;
        public float MaxLength { get; private set; }

        void Awake()
        {
            lineRenderer = GetComponent<LineRenderer>();
            lineRenderer.positionCount = 2;

            if (playerA == null || playerB == null)
                throw new System.Exception("RopeSystem: both players must be assigned");

            MaxLength = playerA.Config.MaxRopeLength;

            Collider2D colA = playerA.GetComponent<Collider2D>();
            Collider2D colB = playerB.GetComponent<Collider2D>();
            Physics2D.IgnoreCollision(colA, colB, true);
        }

        void Update()
        {
            lineRenderer.SetPosition(0, playerA.transform.position);
            lineRenderer.SetPosition(1, playerB.transform.position);
        }

        void FixedUpdate()
        {
            EnforceRopeConstraint();
        }

        void EnforceRopeConstraint()
        {
            Rigidbody2D rbA = playerA.GetComponent<Rigidbody2D>();
            Rigidbody2D rbB = playerB.GetComponent<Rigidbody2D>();

            Vector2 posA = rbA.position;
            Vector2 posB = rbB.position;
            Vector2 delta = posA - posB;
            float dist = delta.magnitude;

            if (dist <= MaxLength || dist < 0.001f)
                return;

            Vector2 dir = delta / dist;
            bool aAnchor = playerA.Form == CharacterForm.Anchor;
            bool bAnchor = playerB.Form == CharacterForm.Anchor;

            if (aAnchor && bAnchor)
                return;

            if (aAnchor)
            {
                rbB.position = posA - dir * MaxLength;
                ProjectOutward(rbB, -dir);
            }
            else if (bAnchor)
            {
                rbA.position = posB + dir * MaxLength;
                ProjectOutward(rbA, dir);
            }
            else
            {
                Vector2 mid = (posA + posB) * 0.5f;
                rbA.position = mid + dir * MaxLength * 0.5f;
                rbB.position = mid - dir * MaxLength * 0.5f;
                ProjectOutward(rbA, dir);
                ProjectOutward(rbB, -dir);
            }
        }

        void ProjectOutward(Rigidbody2D rb, Vector2 awayFromCenter)
        {
            float outward = Vector2.Dot(rb.velocity, awayFromCenter);
            if (outward > 0f)
                rb.velocity -= awayFromCenter * outward;
        }
    }
}
