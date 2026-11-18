using UnityEngine;

namespace CGJ2026
{
    [RequireComponent(typeof(Collider2D))]
    public class DeathZone : MonoBehaviour
    {
        void Awake()
        {
            var col = GetComponent<Collider2D>();
            if (!col.isTrigger)
                throw new System.Exception("DeathZone collider must be set to IsTrigger");
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (other.GetComponent<PlayerController>() == null) return;
            EventBus.Emit(E.PlayerFell);
        }
    }
}
