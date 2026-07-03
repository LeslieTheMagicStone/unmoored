using DG.Tweening;
using UnityEngine;

namespace CGJ2026
{
    [RequireComponent(typeof(LineRenderer))]
    public class RopeSystem : MonoBehaviour
    {
        [SerializeField] PlayerController playerA;
        [SerializeField] PlayerController playerB;
        [SerializeField] float tautTolerance = 0.15f;
        [SerializeField] float reelSpeed = 4f;
        [SerializeField] float minReelLength = 1f;

        LineRenderer lineRenderer;
        DistanceJoint2D ropeJoint;
        Rigidbody2D bodyA;
        Rigidbody2D bodyB;
        float currentLength;
        bool reeling;

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
            currentLength = MaxLength;

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

            if (reeling)
                currentLength = Mathf.MoveTowards(currentLength, minReelLength, reelSpeed * Time.deltaTime);
            else
                currentLength = Mathf.MoveTowards(currentLength, MaxLength, reelSpeed * Time.deltaTime);

            ropeJoint.distance = currentLength;
        }

        public bool IsTaut()
        {
            return Vector2.Distance(bodyA.position, bodyB.position) >= currentLength - tautTolerance;
        }

        public void StartReel() => reeling = true;
        public void StopReel() => reeling = false;

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

            float inputStrength = Mathf.Abs(horizontalInput);
            Vector2 force = player.Config.SwingForce * inputStrength * Mathf.Sign(horizontalInput) * Vector2.right;
            playerBody.AddForce(force, ForceMode2D.Force);

            PlayerController otherPlayer = isPlayerA ? playerB : playerA;
            if (otherPlayer.Form == CharacterForm.Human && otherBody.bodyType == RigidbodyType2D.Dynamic)
            {
                Vector2 reaction = -Mathf.Sign(horizontalInput) * player.Config.SwingReactiveForce * inputStrength * Vector2.right;
                otherBody.AddForce(reaction, ForceMode2D.Force);
            }
            else if (otherPlayer.Form == CharacterForm.Human && otherPlayer.IsGrounded)
            {
                PullGroundedPlayerTowardEdge(otherPlayer);
            }

            return true;
        }

        void PullGroundedPlayerTowardEdge(PlayerController grounded)
        {
            Rigidbody2D groundedRb = grounded.Body;
            PlayerController swinger = grounded == playerA ? playerB : playerA;
            Vector2 dirToSwinger = ((Vector2)swinger.Body.position - groundedRb.position).normalized;

            RaycastHit2D edgeHit = Physics2D.Raycast(groundedRb.position, dirToSwinger, MaxLength + 1f, grounded.GroundLayer);
            if (!edgeHit)
                return;

            Vector2 edgePoint = edgeHit.point;
            Vector2 outFromWall = (edgePoint - (Vector2)groundedRb.position).normalized;

            float horizontal = Mathf.Sign(outFromWall.x);
            Vector2 force = new Vector2(horizontal, 0f) * grounded.Config.SwingReactiveForce * 0.8f;
            groundedRb.AddForce(force, ForceMode2D.Force);
        }

        void ConfigureRopeJoint()
        {
            ropeJoint.autoConfigureDistance = false;
            ropeJoint.autoConfigureConnectedAnchor = false;
            ropeJoint.anchor = Vector2.zero;
            ropeJoint.connectedAnchor = Vector2.zero;
            ropeJoint.connectedBody = bodyB;
            ropeJoint.distance = currentLength;
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
