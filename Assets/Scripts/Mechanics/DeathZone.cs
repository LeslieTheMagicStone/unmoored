using UnityEngine;

namespace CGJ2026
{
    /// <summary>Kills the pair when either player enters (bottom boundary, spikes, water).</summary>
    [RequireComponent(typeof(Collider2D))]
    public class DeathZone : MonoBehaviour
    {
        void Awake()
        {
            if (!GetComponent<Collider2D>().isTrigger)
                throw new System.Exception("DeathZone collider must be set to IsTrigger");
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            var pc = other.GetComponent<PlayerController>();
            if (pc != null)
                EventBus.Emit(E.PlayerFell, pc);
        }
    }
}
