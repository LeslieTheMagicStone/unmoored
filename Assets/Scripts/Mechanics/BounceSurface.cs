using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace CGJ2026
{
    /// <summary>Trampoline: launches a player who lands on top of it straight up.</summary>
    [RequireComponent(typeof(Collider2D))]
    public class BounceSurface : MonoBehaviour
    {
        const float TopContactTolerance = 0.05f;
        const float RecoverDuration = 0.1f;

        [SerializeField] float bounceForce = 25f;
        [SerializeField] float cooldown = 0.3f;
        [SerializeField] Transform visualTransform;
        [SerializeField] float squashDuration = 0.1f;
        [SerializeField] float stretchAmount = 0.2f;

        Collider2D surface;
        Vector3 visualRestScale;
        readonly Dictionary<PlayerController, float> lastBounceTime = new();

        void Awake()
        {
            surface = GetComponent<Collider2D>();
            if (visualTransform != null)
                visualRestScale = visualTransform.localScale;
        }

        void OnDestroy()
        {
            if (visualTransform != null)
                visualTransform.DOKill();
        }

        void OnCollisionStay2D(Collision2D collision)
        {
            var pc = collision.gameObject.GetComponent<PlayerController>();
            if (pc == null || !IsOnTop(collision.collider))
                return;

            if (lastBounceTime.TryGetValue(pc, out float last) && Time.time - last < cooldown)
                return;
            lastBounceTime[pc] = Time.time;

            Rigidbody2D rb = pc.Body;
            rb.velocity = new Vector2(rb.velocity.x, 0f);
            rb.AddForce(Vector2.up * bounceForce, ForceMode2D.Impulse);
            AudioManager.Instance.PlayBounce();
            PlaySquash();
        }

        bool IsOnTop(Collider2D other)
        {
            return other.bounds.min.y >= surface.bounds.max.y - TopContactTolerance;
        }

        void PlaySquash()
        {
            if (visualTransform == null)
                return;

            visualTransform.DOKill();
            visualTransform.DOScaleY(visualRestScale.y - stretchAmount, squashDuration)
                .OnComplete(() => visualTransform.DOScaleY(visualRestScale.y, RecoverDuration).SetEase(Ease.OutBack));
        }
    }
}
