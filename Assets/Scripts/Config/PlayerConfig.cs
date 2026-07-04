using UnityEngine;

namespace CGJ2026
{
    [CreateAssetMenu(menuName = "CGJ2026/PlayerConfig")]
    public class PlayerConfig : ScriptableObject
    {
        [SerializeField] float moveSpeed = 5f;
        [SerializeField] float jumpForce = 10f;
        [SerializeField] float maxRopeLength = 6f;
        [SerializeField] float groundCheckRadius = 0.1f;
        [SerializeField] float gravityScale = 3f;
        [SerializeField] float coyoteTime = 0.08f;
        [SerializeField] float humanMass = 1f;
        [SerializeField] float anchorMass = 1000f;
        [SerializeField] float humanDrag = 0.5f;
        [SerializeField] float anchorDrag = 20f;
        [SerializeField] Vector2 anchorProbeSize = new(1.4f, 1.4f);
        [SerializeField] float anchorEdgeTolerance = 0.8f;
        [SerializeField] float anchorVerticalTolerance = 0.4f;
        [SerializeField] float swingForce = 12f;
        [SerializeField] float swingReactiveForce = 8f;
        [SerializeField] int maxLives = 3;
        [SerializeField] float respawnHeightOffset = 2f;
        [SerializeField] float groundFriction = 1f;
        [SerializeField] float groundBounciness = 0f;
        [SerializeField] float ledgeAssistHeight = 0.35f;
        [SerializeField] float jumpBufferTime = 0.1f;
        [SerializeField] float jumpCutMultiplier = 0.5f;

        public float MoveSpeed => moveSpeed;
        public float JumpForce => jumpForce;
        public float MaxRopeLength => maxRopeLength;
        public float GroundCheckRadius => groundCheckRadius;
        public float GravityScale => gravityScale;
        public float CoyoteTime => coyoteTime;
        public float HumanDrag => humanDrag;
        public float AnchorDrag => anchorDrag;
        public Vector2 AnchorProbeSize => anchorProbeSize;
        public float AnchorEdgeTolerance => anchorEdgeTolerance;
        public float AnchorVerticalTolerance => anchorVerticalTolerance;
        public float HumanMass => humanMass;
        public float AnchorMass => anchorMass;
        public float SwingForce => swingForce;
        public float SwingReactiveForce => swingReactiveForce;
        public int MaxLives => maxLives;
        public float RespawnHeightOffset => respawnHeightOffset;
        public float GroundFriction => groundFriction;
        public float GroundBounciness => groundBounciness;
        public float LedgeAssistHeight => ledgeAssistHeight;
        public float JumpBufferTime => jumpBufferTime;
        public float JumpCutMultiplier => jumpCutMultiplier;
    }
}
