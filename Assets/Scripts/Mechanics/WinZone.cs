using System.Collections.Generic;
using UnityEngine;

namespace CGJ2026
{
    /// <summary>Completes the level once both players stand inside the zone at the same time.</summary>
    [RequireComponent(typeof(Collider2D))]
    public class WinZone : MonoBehaviour
    {
        const int RequiredPlayers = 2;

        readonly HashSet<PlayerController> playersInside = new();
        bool completed;

        void Awake()
        {
            if (!GetComponent<Collider2D>().isTrigger)
                throw new System.Exception("WinZone collider must be set to IsTrigger");
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            var pc = other.GetComponent<PlayerController>();
            if (pc == null || completed)
                return;

            playersInside.Add(pc);
            if (playersInside.Count < RequiredPlayers)
                return;

            completed = true;
            EventBus.Emit(E.LevelComplete);
        }

        void OnTriggerExit2D(Collider2D other)
        {
            var pc = other.GetComponent<PlayerController>();
            if (pc != null)
                playersInside.Remove(pc);
        }
    }
}
