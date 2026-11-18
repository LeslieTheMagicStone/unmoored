using UnityEngine;

namespace CGJ2026
{
    [RequireComponent(typeof(Collider2D))]
    public class WinZone : MonoBehaviour
    {
        void Awake()
        {
            var col = GetComponent<Collider2D>();
            if (!col.isTrigger)
                throw new System.Exception("WinZone collider must be set to IsTrigger");
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            var pc = other.GetComponent<PlayerController>();
            if (pc == null) return;

            LevelManager.Instance?.PlayerReachedWin(pc.PlayerIndex);
        }
    }
}
