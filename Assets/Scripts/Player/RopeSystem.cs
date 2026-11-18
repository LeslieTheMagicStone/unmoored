using UnityEngine;

namespace CGJ2026
{
    /// <summary>
    /// Fixed-length rope between the two players: a max-distance joint plus a line renderer.
    /// Also turns a hanging player's horizontal input into swing force.
    /// </summary>
    [RequireComponent(typeof(LineRenderer))]
    public class RopeSystem : MonoBehaviour
    {
        static readonly Color RopeColor = new(0.8f, 0.65f, 0.2f, 1f);
        const float RopeWidth = 0.06f;

        static Material ropeMaterial;

        [SerializeField] PlayerController playerA;
        [SerializeField] PlayerController playerB;
        [SerializeField] float tautTolerance = 0.15f;

        LineRenderer lineRenderer;

        public PlayerController PlayerA => playerA;
        public PlayerController PlayerB => playerB;
        public float MaxLength { get; private set; }

        public void SetVisible(bool visible)
        {
            lineRenderer.enabled = visible;
        }

        public bool IsTaut()
        {
            return Vector2.Distance(playerA.Body.position, playerB.Body.position) >= MaxLength - tautTolerance;
        }

        /// <summary>
        /// Applies swing force for <paramref name="player"/> while the rope is taut.
        /// Returns false when the player is not swinging, so the caller can fall back to air control.
        /// A human partner feels the opposite reaction and can be dragged off a ledge.
        /// </summary>
        public bool TryApplySwingInput(PlayerController player, float horizontalInput)
        {
            if (player == null)
                throw new System.ArgumentNullException(nameof(player));
            if (player != playerA && player != playerB)
                throw new System.ArgumentException("Player is not connected to this rope.", nameof(player));

            if (horizontalInput == 0f || player.Form == CharacterForm.Anchor || !IsTaut())
                return false;

            float direction = Mathf.Sign(horizontalInput);
            float strength = Mathf.Abs(horizontalInput);
            player.Body.AddForce(player.Config.SwingForce * strength * direction * Vector2.right, ForceMode2D.Force);

            PlayerController partner = player == playerA ? playerB : playerA;
            if (partner.Form == CharacterForm.Human)
            {
                Vector2 reaction = player.Config.SwingReactiveForce * strength * -direction * Vector2.right;
                partner.Body.AddForce(reaction, ForceMode2D.Force);
            }

            return true;
        }

        void Awake()
        {
            if (playerA == null || playerB == null)
                throw new System.Exception("RopeSystem: both players must be assigned");

            if (ropeMaterial == null)
                ropeMaterial = new Material(Shader.Find("Sprites/Default"));

            lineRenderer = GetComponent<LineRenderer>();
            lineRenderer.material = ropeMaterial;
            lineRenderer.startColor = lineRenderer.endColor = RopeColor;
            lineRenderer.startWidth = lineRenderer.endWidth = RopeWidth;
            lineRenderer.positionCount = 2;

            MaxLength = playerA.Config.MaxRopeLength;
            CreateJoint();

            Physics2D.IgnoreCollision(playerA.GetComponent<Collider2D>(), playerB.GetComponent<Collider2D>(), true);

            playerA.BindRope(this);
            playerB.BindRope(this);
        }

        void LateUpdate()
        {
            lineRenderer.SetPosition(0, playerA.transform.position);
            lineRenderer.SetPosition(1, playerB.transform.position);
        }

        void CreateJoint()
        {
            var joint = playerA.GetComponent<DistanceJoint2D>();
            if (joint == null)
                joint = playerA.gameObject.AddComponent<DistanceJoint2D>();

            joint.autoConfigureDistance = false;
            joint.autoConfigureConnectedAnchor = false;
            joint.anchor = Vector2.zero;
            joint.connectedAnchor = Vector2.zero;
            joint.connectedBody = playerB.GetComponent<Rigidbody2D>();
            joint.distance = MaxLength;
            joint.maxDistanceOnly = true;
            joint.enableCollision = false;
        }
    }
}
