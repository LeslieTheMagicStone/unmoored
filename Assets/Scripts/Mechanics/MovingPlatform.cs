using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace CGJ2026
{
    /// <summary>
    /// Loops through waypoints, carrying grounded players with it.
    /// Pauses while any player is anchored to it, because an anchored player is
    /// kinematic and would otherwise be left floating in place.
    /// </summary>
    [DefaultExecutionOrder(-50)]
    public class MovingPlatform : MonoBehaviour
    {
        [SerializeField] Transform[] waypoints = new Transform[2];
        [SerializeField] float moveSpeed = 2f;
        [SerializeField] float pauseAtEnds = 0.5f;
        [Tooltip("Players anchored inside this area count as anchored to the platform. " +
                 "Leave empty to count only players anchored to the platform's own colliders.")]
        [SerializeField] Collider2D anchorZone;

        Tween moveTween;
        int currentIndex;
        Vector3 previousPosition;
        readonly HashSet<PlayerController> riders = new();
        readonly HashSet<PlayerController> anchoredPlayers = new();

        void Awake()
        {
            if (waypoints.Length < 2)
                throw new System.Exception($"MovingPlatform [{name}]: needs at least 2 waypoints");
            for (int i = 0; i < waypoints.Length; i++)
            {
                if (waypoints[i] == null)
                    throw new System.Exception($"MovingPlatform [{name}]: waypoint {i} is not assigned");
            }
            if (moveSpeed <= 0f)
                throw new System.Exception($"MovingPlatform [{name}]: moveSpeed must be > 0");
        }

        void Start()
        {
            transform.position = waypoints[0].position;
            previousPosition = transform.position;
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

        void OnDestroy()
        {
            moveTween?.Kill();
        }

        void MoveToNext()
        {
            currentIndex = (currentIndex + 1) % waypoints.Length;
            Vector3 target = waypoints[currentIndex].position;
            float duration = Vector3.Distance(transform.position, target) / moveSpeed;

            moveTween = transform.DOMove(target, duration)
                .SetEase(Ease.Linear)
                .SetDelay(pauseAtEnds)
                .OnComplete(MoveToNext);
            if (anchoredPlayers.Count > 0)
                moveTween.Pause();
        }

        void OnPlayerAnchored(object data)
        {
            var pc = (PlayerController)data;
            if (!IsAnchoredHere(pc) || !anchoredPlayers.Add(pc))
                return;
            if (anchoredPlayers.Count == 1)
                moveTween.Pause();
        }

        void OnPlayerReleased(object data)
        {
            // Match by identity, not position: on respawn a player is teleported before it is released.
            if (anchoredPlayers.Remove((PlayerController)data) && anchoredPlayers.Count == 0)
                moveTween.Play();
        }

        bool IsAnchoredHere(PlayerController pc)
        {
            Collider2D surface = pc.AnchoredCollider;
            if (surface != null && surface.transform.IsChildOf(transform))
                return true;
            return anchorZone != null && anchorZone.bounds.Contains(pc.transform.position);
        }

        void FixedUpdate()
        {
            Vector3 delta = transform.position - previousPosition;
            previousPosition = transform.position;
            if (delta == Vector3.zero)
                return;

            foreach (var pc in riders)
            {
                if (!pc.IsAnchored && pc.IsGrounded)
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
