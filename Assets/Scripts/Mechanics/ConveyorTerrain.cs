using UnityEngine;

namespace CGJ2026
{
    /// <summary>
    /// Moves terrain diagonally. Grounded players inherit CurrentVelocity via PlayerController.
    /// Assign Move Root to an empty object parenting all scrolling terrain, or leave empty to move this object only.
    /// </summary>
    [DefaultExecutionOrder(-50)]
    public class ConveyorTerrain : MonoBehaviour
    {
        [SerializeField] Transform moveRoot;
        [SerializeField] Vector2 direction = new(-1f, -1f);
        [SerializeField] float speed = 1.5f;

        int anchoredCount;

        Transform Root => moveRoot != null ? moveRoot : transform;

        public Vector2 CurrentVelocity => anchoredCount > 0 ? Vector2.zero : direction.normalized * speed;

        void Awake()
        {
            if (direction.sqrMagnitude < 0.0001f)
                throw new System.Exception($"ConveyorTerrain [{name}]: direction must not be zero");
        }

        void OnEnable()
        {
            EventBus.On(E.PlayerAnchored, OnPlayerAnchored);
            EventBus.On(E.PlayerReleased, OnPlayerReleased);
        }

        void OnDisable()
        {
            EventBus.Off(E.PlayerAnchored, OnPlayerAnchored);
            EventBus.Off(E.PlayerReleased, OnPlayerReleased);
        }

        void FixedUpdate()
        {
            Vector2 velocity = CurrentVelocity;
            if (velocity.sqrMagnitude <= 0f)
                return;

            Root.position += (Vector3)(velocity * Time.fixedDeltaTime);
        }

        public bool ContainsCollider(Collider2D col)
        {
            if (col == null)
                return false;

            Transform root = Root;
            Transform t = col.transform;
            return t == root || t.IsChildOf(root);
        }

        void OnPlayerAnchored(object data)
        {
            var pc = data as PlayerController;
            if (pc == null || !IsTouchingRoot(pc))
                return;

            anchoredCount++;
        }

        void OnPlayerReleased(object _)
        {
            anchoredCount = Mathf.Max(0, anchoredCount - 1);
        }

        bool IsTouchingRoot(PlayerController pc)
        {
            if (pc.GetConveyorUnderfoot() == this)
                return true;

            var col = pc.GetComponent<Collider2D>();
            if (col == null)
                return false;

            var hits = Physics2D.OverlapBoxAll(col.bounds.center, col.bounds.size, 0f, pc.GroundLayer);
            foreach (var hit in hits)
            {
                if (ContainsCollider(hit))
                    return true;
            }

            return false;
        }
    }
}
