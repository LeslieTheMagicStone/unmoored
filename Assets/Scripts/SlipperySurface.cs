using UnityEngine;

namespace CGJ2026
{
    public class SlipperySurface : MonoBehaviour
    {
        [SerializeField] float slideSpeed = 0.5f;

        Collider2D surfaceCollider;

        void Awake()
        {
            surfaceCollider = GetComponent<Collider2D>();
        }

        void FixedUpdate()
        {
            if (surfaceCollider == null) return;

            Bounds b = surfaceCollider.bounds;
            b.Expand(0.2f);

            var results = Physics2D.OverlapBoxAll(b.center, b.size, 0f);

            foreach (var col in results)
            {
                var pc = col.GetComponent<PlayerController>();
                if (pc == null || !pc.IsAnchored) continue;

                pc.Body.position += slideSpeed * Time.fixedDeltaTime * Vector2.down;
            }
        }
    }
}

