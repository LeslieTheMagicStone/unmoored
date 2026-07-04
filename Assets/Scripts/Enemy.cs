using UnityEngine;

namespace CGJ2026
{
    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
    public class Enemy : MonoBehaviour
    {
        [Header("Combat")]
        [SerializeField] float stompThreshold = -2f;
        [SerializeField] float slamSpeedThreshold = 8f;
        [SerializeField] float slamForce = 15f;
        [SerializeField] float stompBounceForce = 8f;

        [Header("Patrol")]
        [SerializeField] float patrolSpeed = 1.5f;
        [SerializeField] LayerMask groundLayer;
        [SerializeField] LayerMask wallLayer;

        Rigidbody2D rb;
        Collider2D col;
        int facing = 1; // 1 = right, -1 = left

        void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            col = GetComponent<Collider2D>();
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.mass = 2f;
        }

        void FixedUpdate()
        {
            Patrol();
        }

        void Patrol()
        {
            Bounds b = col.bounds;
            float halfW = b.extents.x;
            float halfH = b.extents.y;
            float skin = 0.05f; // small offset to avoid hitting own collider

            // Ground check: only checks ground layer
            Vector2 frontOrigin = new Vector2(b.center.x + (halfW + skin) * facing, b.center.y);
            bool groundAhead = Physics2D.Raycast(frontOrigin, Vector2.down, halfH + 0.1f, groundLayer);

            // Wall check: checks wall layer (should include ground + enemy layers)
            Vector2 wallOrigin = new Vector2(b.center.x + (halfW + skin) * facing, b.center.y);
            bool wallAhead = Physics2D.Raycast(wallOrigin, Vector2.right * facing, 0.3f, wallLayer);

            if (!groundAhead || wallAhead)
                facing *= -1;

            rb.velocity = new Vector2(facing * patrolSpeed, rb.velocity.y);
        }

        void OnCollisionEnter2D(Collision2D collision)
        {
            var pc = collision.gameObject.GetComponent<PlayerController>();
            if (pc == null) return;

            var playerRb = collision.rigidbody;
            if (playerRb == null) return;

            float playerSpeed = playerRb.velocity.magnitude;

            if (playerRb.velocity.y < stompThreshold)
            {
                Stomped(playerRb);
                return;
            }

            if (playerSpeed >= slamSpeedThreshold)
            {
                BodySlammed(collision);
                return;
            }

            EventBus.Emit(E.PlayerFell);
        }

        void Stomped(Rigidbody2D playerRb)
        {
            playerRb.velocity = new Vector2(playerRb.velocity.x, 0f);
            playerRb.AddForce(Vector2.up * stompBounceForce, ForceMode2D.Impulse);
            Destroy(gameObject);
        }

        void BodySlammed(Collision2D collision)
        {
            Vector2 dir = ((Vector2)transform.position - (Vector2)collision.transform.position).normalized;
            rb.AddForce(dir * slamForce, ForceMode2D.Impulse);
        }
    }
}
