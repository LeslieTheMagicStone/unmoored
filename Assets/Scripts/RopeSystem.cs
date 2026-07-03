using UnityEngine;

namespace CGJ2026
{
    [RequireComponent(typeof(LineRenderer))]
    public class RopeSystem : MonoBehaviour
    {
        [SerializeField] PlayerController playerA;
        [SerializeField] PlayerController playerB;
        [SerializeField] float tautTolerance = 0.15f;

        LineRenderer lineRenderer;
        DistanceJoint2D ropeJoint;
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

            ropeJoint = playerA.GetComponent<DistanceJoint2D>();
            if (ropeJoint == null)
                ropeJoint = playerA.gameObject.AddComponent<DistanceJoint2D>();

            ConfigureRopeJoint();

            Physics2D.IgnoreCollision(
                playerA.GetComponent<Collider2D>(),
                playerB.GetComponent<Collider2D>(),
                true);
        }

        void Update()
        {
            DrawRope();
        }

        public bool IsTaut()
        {
            return Vector2.Distance(bodyA.position, bodyB.position) >= MaxLength - tautTolerance;
        }

        public bool TryApplySwingInput(PlayerController player, float horizontalInput)
        {
            if (player == null)
                throw new System.ArgumentNullException(nameof(player));
            if (player != playerA && player != playerB)
                throw new System.ArgumentException("Player is not connected to this rope.", nameof(player));
            if (Mathf.Approximately(horizontalInput, 0f)) return false;
            if (player.Form == CharacterForm.Anchor) return false;
            if (!IsTaut()) return false;

            bool isPlayerA = player == playerA;
            Rigidbody2D playerBody = isPlayerA ? bodyA : bodyB;
            Rigidbody2D otherBody = isPlayerA ? bodyB : bodyA;

            Vector2 ropeVector = playerBody.position - otherBody.position;
            Vector2 tangent = new(-ropeVector.y, ropeVector.x);
            if (tangent.sqrMagnitude <= 0.0001f) return false;

            tangent.Normalize();
            if (Vector2.Dot(tangent, Vector2.right) * horizontalInput < 0f)
                tangent = -tangent;

            float inputStrength = Mathf.Abs(horizontalInput);
            Vector2 force = player.Config.SwingForce * inputStrength * tangent;
            playerBody.AddForce(force, ForceMode2D.Force);

            PlayerController otherPlayer = isPlayerA ? playerB : playerA;
            if (otherPlayer.Form == CharacterForm.Human && otherBody.bodyType == RigidbodyType2D.Dynamic)
            {
                Vector2 reaction = -force.normalized * player.Config.SwingReactiveForce * inputStrength;
                otherBody.AddForce(reaction, ForceMode2D.Force);
            }

            return true;
        }

        void ConfigureRopeJoint()
        {
            ropeJoint.autoConfigureDistance = false;
            ropeJoint.autoConfigureConnectedAnchor = false;
            ropeJoint.anchor = Vector2.zero;
            ropeJoint.connectedAnchor = Vector2.zero;
            ropeJoint.connectedBody = bodyB;
            ropeJoint.distance = MaxLength;
            ropeJoint.maxDistanceOnly = true;
            ropeJoint.enableCollision = false;
        }

        void DrawRope()
        {
            lineRenderer.positionCount = 2;
            lineRenderer.SetPosition(0, playerA.transform.position);
            lineRenderer.SetPosition(1, playerB.transform.position);
        }
    }
}
