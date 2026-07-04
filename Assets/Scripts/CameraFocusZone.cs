using UnityEngine;

namespace CGJ2026
{
    [RequireComponent(typeof(Collider2D))]
    public class CameraFocusZone : MonoBehaviour
    {
        [SerializeField] Transform focusPoint;
        [SerializeField] bool overrideSize;
        [SerializeField] float targetSize = 4f;
        [SerializeField] float exitMargin = 2f;

        new Collider2D collider2D;
        int playersInside;
        const int TotalPlayers = 2;
        Vector2 focusPosition;
        bool active;

        void Awake()
        {
            collider2D = GetComponent<Collider2D>();
            collider2D.isTrigger = true;
        }

        void Start()
        {
            focusPosition = focusPoint != null ? focusPoint.position : transform.position;
        }

        void LateUpdate()
        {
            if (!active) return;

            Bounds b = collider2D.bounds;
            b.Expand(exitMargin);

            var players = GameObject.FindGameObjectsWithTag("Player");
            foreach (var p in players)
            {
                if (b.Contains(p.transform.position))
                    return;
            }

            active = false;
            CameraSetup.FocusOverride = null;
            CameraSetup.SizeOverride = null;
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Player")) return;
            playersInside++;
            if (playersInside >= TotalPlayers)
            {
                active = true;
                CameraSetup.FocusOverride = focusPosition;
                if (overrideSize)
                    CameraSetup.SizeOverride = targetSize;
            }
        }

        void OnTriggerExit2D(Collider2D other)
        {
            if (!other.CompareTag("Player")) return;
            playersInside = Mathf.Max(0, playersInside - 1);
        }

        void OnDrawGizmos()
        {
            var c = GetComponent<Collider2D>();
            if (c != null)
            {
                Bounds b = c.bounds;
                Gizmos.color = new Color(1f, 0.9f, 0f, 0.15f);
                Gizmos.DrawCube(b.center, b.size + Vector3.one * exitMargin * 2f);
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireCube(b.center, b.size + Vector3.one * exitMargin * 2f);
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
