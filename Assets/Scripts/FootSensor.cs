using UnityEngine;

namespace CGJ2026
{
    public class FootSensor : MonoBehaviour
    {
        int contactCount;
        LayerMask groundLayer;

        public bool IsGrounded => contactCount > 0;

        public void Init(LayerMask layer)
        {
            groundLayer = layer;
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if ((groundLayer.value & (1 << other.gameObject.layer)) != 0)
                contactCount++;
        }

        void OnTriggerExit2D(Collider2D other)
        {
            if ((groundLayer.value & (1 << other.gameObject.layer)) != 0)
                contactCount = Mathf.Max(0, contactCount - 1);
        }
    }
}
