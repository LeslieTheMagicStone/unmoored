using System.Collections.Generic;
using UnityEngine;

namespace CGJ2026
{
    /// <summary>
    /// When both players are inside, locks the camera onto a focus point (and optionally a zoom level).
    /// The lock is released once every player is outside the trigger expanded by <see cref="exitMargin"/>,
    /// which keeps the camera from flickering at the boundary.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class CameraFocusZone : MonoBehaviour
    {
        const int RequiredPlayers = 2;

        [SerializeField] Transform focusPoint;
        [SerializeField] bool overrideSize;
        [SerializeField] float targetSize = 4f;
        [SerializeField] float exitMargin = 2f;

        Collider2D zone;
        readonly HashSet<PlayerController> playersInside = new();
        readonly HashSet<PlayerController> trackedPlayers = new();
        bool active;

        void Awake()
        {
            zone = GetComponent<Collider2D>();
            zone.isTrigger = true;
        }

        void OnDisable()
        {
            Deactivate();
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            var pc = other.GetComponent<PlayerController>();
            if (pc == null)
                return;

            playersInside.Add(pc);
            trackedPlayers.Add(pc);
            if (!active && playersInside.Count >= RequiredPlayers)
                Activate();
        }

        void OnTriggerExit2D(Collider2D other)
        {
            var pc = other.GetComponent<PlayerController>();
            if (pc != null)
                playersInside.Remove(pc);
        }

        void LateUpdate()
        {
            if (!active)
                return;

            Bounds releaseBounds = zone.bounds;
            releaseBounds.Expand(exitMargin);

            foreach (var pc in trackedPlayers)
            {
                if (releaseBounds.Contains(pc.transform.position))
                    return;
            }

            Deactivate();
        }

        void Activate()
        {
            active = true;
            Vector2 point = focusPoint != null ? focusPoint.position : transform.position;
            CameraSetup.SetFocus(this, point, overrideSize ? targetSize : (float?)null);
        }

        void Deactivate()
        {
            if (!active)
                return;
            active = false;
            CameraSetup.ClearFocus(this);
        }

        void OnDrawGizmos()
        {
            var c = GetComponent<Collider2D>();
            if (c != null)
            {
                Bounds b = c.bounds;
                Vector3 size = b.size + Vector3.one * exitMargin * 2f;
                Gizmos.color = new Color(1f, 0.9f, 0f, 0.15f);
                Gizmos.DrawCube(b.center, size);
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireCube(b.center, size);
            }

            if (focusPoint != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(focusPoint.position, 0.15f);
                Gizmos.DrawLine(transform.position, focusPoint.position);
            }
        }
    }
}
