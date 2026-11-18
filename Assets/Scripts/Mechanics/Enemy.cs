using UnityEngine;

namespace CGJ2026
{
    /// <summary>
    /// Patrols a platform, turning at ledges and walls. On contact with a player:
    /// landing on its head kills it, a fast swing slams it away, anything else kills the players.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
    public class Enemy : MonoBehaviour
    {
        const float Mass = 2f;
        const float ProbeSkin = 0.05f;
        const float GroundProbeExtra = 0.1f;
        const float WallProbeReach = 0.3f;

        [Header("Combat")]
        [Tooltip("Vertical impact speed needed for a stomp. Only the magnitude is used, so existing negative values still work.")]
        [SerializeField] float stompThreshold = -2f;
        [Tooltip("Minimum impact speed that slams the enemy away instead of hurting the player.")]
        [SerializeField] float slamSpeedThreshold = 8f;
        [SerializeField] float slamForce = 15f;
        [SerializeField] float stompBounceForce = 8f;

        [Header("Patrol")]
        [SerializeField] float patrolSpeed = 1.5f;
        [SerializeField] LayerMask groundLayer;
        [Tooltip("Layers that turn the enemy around; should include ground and other enemies.")]
        [SerializeField] LayerMask wallLayer;

        Rigidbody2D rb;
        Collider2D body;
        int facing = 1;

        void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            body = GetComponent<Collider2D>();
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.mass = Mass;
        }

        void FixedUpdate()
        {
            Bounds b = body.bounds;
            var front = new Vector2(b.center.x + (b.extents.x + ProbeSkin) * facing, b.center.y);
            bool groundAhead = Physics2D.Raycast(front, Vector2.down, b.extents.y + GroundProbeExtra, groundLayer);
            bool wallAhead = Physics2D.Raycast(front, Vector2.right * facing, WallProbeReach, wallLayer);

            if (!groundAhead || wallAhead)
                facing = -facing;

            rb.velocity = new Vector2(facing * patrolSpeed, rb.velocity.y);
        }

        void OnCollisionEnter2D(Collision2D collision)
        {
            var pc = collision.gameObject.GetComponent<PlayerController>();
            if (pc == null)
                return;

            // Body velocities are already resolved by the time this callback runs (a stomp
            // reads as ~0), so judge the hit by the pre-impact relative velocity instead.
            Vector2 impact = collision.relativeVelocity;
            bool playerAbove = collision.collider.bounds.min.y >= body.bounds.center.y;

            if (playerAbove && Mathf.Abs(impact.y) >= Mathf.Abs(stompThreshold))
                Stomped(pc);
            else if (impact.magnitude >= slamSpeedThreshold)
                Slammed(collision.transform.position);
            else
                EventBus.Emit(E.PlayerFell, pc);
        }

        void Stomped(PlayerController pc)
        {
            AudioManager.Instance.PlayStomp();

            // A human bounces off; an anchor-form player smashes through and keeps falling.
            if (pc.Form == CharacterForm.Human)
            {
                Rigidbody2D playerBody = pc.Body;
                playerBody.velocity = new Vector2(playerBody.velocity.x, 0f);
                playerBody.AddForce(Vector2.up * stompBounceForce, ForceMode2D.Impulse);
            }

            Destroy(gameObject);
        }

        void Slammed(Vector2 hitterPosition)
        {
            Vector2 away = ((Vector2)transform.position - hitterPosition).normalized;
            rb.AddForce(away * slamForce, ForceMode2D.Impulse);
        }
    }
}
