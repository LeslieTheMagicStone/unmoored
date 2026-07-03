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

        public float MoveSpeed => moveSpeed;
        public float JumpForce => jumpForce;
        public float MaxRopeLength => maxRopeLength;
        public float GroundCheckRadius => groundCheckRadius;
    }
}
