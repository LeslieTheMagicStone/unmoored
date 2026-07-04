using UnityEngine;

namespace CGJ2026
{
    [RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
    public class Elevator : MonoBehaviour
    {
        [SerializeField] Transform pointA;
        [SerializeField] Transform pointB;
        [SerializeField] float duration = 2f;

        Rigidbody2D rb;
        bool active;

        void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
        }

        void FixedUpdate()
        {
            Vector3 goal = active ? pointB.position : pointA.position;
            float totalDist = Vector3.Distance(pointA.position, pointB.position);
            float maxStep = totalDist / duration * Time.fixedDeltaTime;
            Vector3 newPos = Vector3.MoveTowards(transform.position, goal, maxStep);
            rb.MovePosition(newPos);
        }

        public void Open()
        {
            active = true;
        }

        public void Close()
        {
            active = false;
        }
    }
}
