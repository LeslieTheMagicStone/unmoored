using UnityEngine;

namespace CGJ2026
{
    [RequireComponent(typeof(Collider2D))]
    public class FootSensor : MonoBehaviour
    {
        LayerMask groundLayer;
        Collider2D footCollider;
        readonly Collider2D[] overlapResults = new Collider2D[8];
        ContactFilter2D contactFilter;

        public bool IsGrounded { get; private set; }

        public void Init(LayerMask layer)
        {
            groundLayer = layer;
            footCollider = GetComponent<Collider2D>();
            contactFilter = new ContactFilter2D();
            contactFilter.SetLayerMask(groundLayer);
            contactFilter.useLayerMask = true;
            contactFilter.useTriggers = false;
        }

        void FixedUpdate()
        {
            if (footCollider == null) return;
            int count = Physics2D.OverlapCollider(footCollider, contactFilter, overlapResults);
            IsGrounded = count > 0;
        }
    }
}
