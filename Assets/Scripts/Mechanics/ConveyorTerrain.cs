using System.Collections.Generic;
using UnityEngine;

namespace CGJ2026
{
    /// <summary>
    /// Moves terrain diagonally. Grounded players inherit <see cref="CurrentVelocity"/> via PlayerController.
    /// Stops while any player is anchored to it, so anchoring is how players hold the terrain still.
    /// Assign Move Root to an empty object parenting all scrolling terrain, or leave empty to move this object only.
    /// </summary>
    [DefaultExecutionOrder(-50)]
    public class ConveyorTerrain : MonoBehaviour
    {
        [SerializeField] Transform moveRoot;
        [SerializeField] Vector2 direction = new(-1f, -1f);
        [SerializeField] float speed = 1.5f;

        readonly HashSet<PlayerController> anchoredPlayers = new();

        Transform Root => moveRoot != null ? moveRoot : transform;

        public Vector2 CurrentVelocity => anchoredPlayers.Count > 0 ? Vector2.zero : direction.normalized * speed;

        void Awake()
        {
            if (direction.sqrMagnitude < 0.0001f)
                throw new System.Exception($"ConveyorTerrain [{name}]: direction must not be zero");
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

        void FixedUpdate()
        {
            Vector2 velocity = CurrentVelocity;
            if (velocity == Vector2.zero)
                return;

            Root.position += (Vector3)(velocity * Time.fixedDeltaTime);
        }

        bool Contains(Collider2D col)
        {
            if (col == null)
                return false;

            Transform root = Root;
            return col.transform == root || col.transform.IsChildOf(root);
        }

        void OnPlayerAnchored(object data)
        {
            var pc = (PlayerController)data;
            if (Contains(pc.AnchoredCollider))
                anchoredPlayers.Add(pc);
        }

        void OnPlayerReleased(object data)
        {
            anchoredPlayers.Remove((PlayerController)data);
        }
    }
}
