using UnityEngine;

namespace CGJ2026
{
    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
    public class Enemy : MonoBehaviour
    {
        [SerializeField] float stompThreshold = -2f;  // player vy must be below this to stomp
        [SerializeField] float slamSpeedThreshold = 8f; // player speed to trigger body slam
        [SerializeField] float slamForce = 15f;
        [SerializeField] float stompBounceForce = 8f;

        Rigidbody2D rb;

        void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.mass = 2f;
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
