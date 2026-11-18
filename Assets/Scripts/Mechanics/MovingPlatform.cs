using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace CGJ2026
{
    [DefaultExecutionOrder(-50)]
    public class MovingPlatform : MonoBehaviour
    {
        [SerializeField] Transform[] waypoints = new Transform[2];
        [SerializeField] float moveSpeed = 2f;
        [SerializeField] float pauseAtEnds = 0.5f;
        [SerializeField] Collider2D anchorZone;

        Tween moveTween;
        int currentIndex;
        int anchoredCount;
        Vector3 previousPosition;
        readonly HashSet<PlayerController> riders = new HashSet<PlayerController>();

        void Start()
        {
            if (waypoints.Length < 2)
            {
                Debug.LogWarning("MovingPlatform needs at least 2 waypoints", this);
                return;
            }

            previousPosition = transform.position;
            transform.position = waypoints[0].position;
            MoveToNext();
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

        void MoveToNext()
        {
            currentIndex = (currentIndex + 1) % waypoints.Length;
            float dist = Vector3.Distance(transform.position, waypoints[currentIndex].position);

            moveTween = transform.DOMove(waypoints[currentIndex].position, dist / moveSpeed)
                .SetEase(Ease.Linear)
                .SetDelay(pauseAtEnds)
                .OnComplete(MoveToNext);
        }

        void OnPlayerAnchored(object data)
        {
            var pc = data as PlayerController;
            if (pc == null) return;
            if (anchorZone != null && !anchorZone.bounds.Contains(pc.transform.position)) return;

            anchoredCount++;
            if (anchoredCount == 1)
                moveTween?.Pause();
        }

        void OnPlayerReleased(object data)
        {
            var pc = data as PlayerController;
            if (pc == null) return;
            if (anchorZone != null && !anchorZone.bounds.Contains(pc.transform.position)) return;

            anchoredCount = Mathf.Max(0, anchoredCount - 1);
            if (anchoredCount == 0)
                moveTween?.Play();
        }

        void FixedUpdate()
        {
            Vector3 delta = transform.position - previousPosition;
            previousPosition = transform.position;

            riders.RemoveWhere(pc => pc == null);
            foreach (var pc in riders)
            {
                if (pc != null && !pc.IsAnchored && pc.IsGrounded)
                    pc.Body.position += (Vector2)delta;
            }
        }

        void OnCollisionEnter2D(Collision2D collision)
        {
            var pc = collision.gameObject.GetComponent<PlayerController>();
            if (pc != null)
                riders.Add(pc);
        }

        void OnCollisionExit2D(Collision2D collision)
        {
            var pc = collision.gameObject.GetComponent<PlayerController>();
            if (pc != null)
                riders.Remove(pc);
        }
    }
}
