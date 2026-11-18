using UnityEngine;

namespace CGJ2026
{
    /// <summary>Kinematic platform that travels to point B while its switch is held, and back to A otherwise.</summary>
    [RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
    public class Elevator : MonoBehaviour
    {
        [SerializeField] Transform pointA;
        [SerializeField] Transform pointB;
        [Tooltip("Seconds for a full trip from A to B.")]
        [SerializeField] float duration = 2f;

        Rigidbody2D rb;
        bool active;

        public void Open() => active = true;
        public void Close() => active = false;

        void Awake()
        {
            if (pointA == null || pointB == null)
                throw new System.Exception($"Elevator [{name}]: pointA and pointB must be assigned");
            if (duration <= 0f)
                throw new System.Exception($"Elevator [{name}]: duration must be > 0");

            rb = GetComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
        }

        void FixedUpdate()
        {
            // Recomputed each step so the points can be moved while tuning in Play mode.
            float speed = Vector2.Distance(pointA.position, pointB.position) / duration;
            Vector2 goal = active ? pointB.position : pointA.position;
            rb.MovePosition(Vector2.MoveTowards(rb.position, goal, speed * Time.fixedDeltaTime));
        }
    }
}
