using UnityEngine;

namespace CGJ2026
{
    [RequireComponent(typeof(Collider2D))]
    public class AnchorPoint : MonoBehaviour
    {
        void Awake()
        {
            var col = GetComponent<Collider2D>();
            if (!col.isTrigger)
                throw new System.Exception("AnchorPoint collider must be IsTrigger");
        }
    }
}
