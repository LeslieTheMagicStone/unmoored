using UnityEngine;

namespace CGJ2026
{
    /// <summary>Tuning values shared by both players and the rope.</summary>
    [CreateAssetMenu(menuName = "CGJ2026/PlayerConfig")]
    public class PlayerConfig : ScriptableObject
    {
        [SerializeField] float moveSpeed = 5f;
        [SerializeField] float jumpForce = 10f;
        [SerializeField] float maxRopeLength = 6f;
        [SerializeField] float gravityScale = 3f;
        [SerializeField] float coyoteTime = 0.08f;
        [SerializeField] float humanMass = 1f;
        [SerializeField] float anchorMass = 1000f;
        [SerializeField] float humanDrag = 0.5f;
        [SerializeField] float anchorDrag = 20f;
        [SerializeField] float swingForce = 12f;
        [SerializeField] float swingReactiveForce = 8f;
        [SerializeField] float wallJumpHorizontalForce = 8f;
        [SerializeField] int maxLives = 3;
        [SerializeField] float respawnHeightOffset = 2f;

        public float MoveSpeed => moveSpeed;
        public float JumpForce => jumpForce;
        public float MaxRopeLength => maxRopeLength;
        public float GravityScale => gravityScale;
        public float CoyoteTime => coyoteTime;
        public float HumanDrag => humanDrag;
        public float AnchorDrag => anchorDrag;
        public float HumanMass => humanMass;
        public float AnchorMass => anchorMass;
        public float SwingForce => swingForce;
        public float SwingReactiveForce => swingReactiveForce;
        public float WallJumpHorizontalForce => wallJumpHorizontalForce;
        public int MaxLives => maxLives;
        public float RespawnHeightOffset => respawnHeightOffset;
    }
}
