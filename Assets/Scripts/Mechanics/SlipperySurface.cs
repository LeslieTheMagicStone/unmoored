using System.Collections.Generic;
using UnityEngine;

namespace CGJ2026
{
    /// <summary>
    /// A surface that cannot hold an anchor: players anchored to its side or underside slowly
    /// slide down and lose the pivot once they slip past the bottom edge.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class SlipperySurface : MonoBehaviour
    {
        [SerializeField] float slideSpeed = 0.5f;

        Collider2D surface;
        readonly HashSet<PlayerController> anchoredPlayers = new();
        readonly List<PlayerController> slidOff = new();

        void Awake()
        {
            surface = GetComponent<Collider2D>();
        }

        void OnEnable()
        {
            EventBus.On(E.PlayerAnchored, OnPlayerAnchored);
            EventBus.On(E.PlayerReleased, OnPlayerReleased);
        }

        void OnDisable()
        {
            EventBus.Off(E.PlayerAnchored, OnPlayerAnchored);
            EventBus.Off(E.PlayerReleased, OnPlayerReleased);
        }

        void OnPlayerAnchored(object data)
        {
            var pc = (PlayerController)data;
            if (pc.AnchoredCollider == surface)
                anchoredPlayers.Add(pc);
        }

        void OnPlayerReleased(object data)
        {
            anchoredPlayers.Remove((PlayerController)data);
        }

        void FixedUpdate()
        {
            Bounds b = surface.bounds;
            slidOff.Clear();
            foreach (var pc in anchoredPlayers)
            {
                float y = pc.Body.position.y;
                // Standing on top is stable. Side anchors slide down; an underside anchor cannot hold at all.
                if (y >= b.max.y)
                    continue;
                if (y < b.min.y)
                    slidOff.Add(pc);
                else
                    pc.Body.MovePosition(pc.Body.position + slideSpeed * Time.fixedDeltaTime * Vector2.down);
            }

            // Detaching raises PlayerReleased, which edits anchoredPlayers, so do it after the loop.
            foreach (var pc in slidOff)
                pc.DetachFromSurface();
        }
    }
}
