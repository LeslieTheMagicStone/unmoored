using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace CGJ2026
{
    public class BounceSurface : MonoBehaviour
    {
        [SerializeField] float bounceForce = 25f;
        [SerializeField] float cooldown = 0.3f;
        [SerializeField] Transform visualTransform;
        [SerializeField] float squashDuration = 0.1f;
        [SerializeField] float stretchAmount = 0.2f;

        Collider2D surfaceCollider;
        Vector3 visualOrigin;
        Dictionary<PlayerController, float> lastBounce = new();

        void Awake()
        {
            surfaceCollider = GetComponent<Collider2D>();
            if (visualTransform != null)
                visualOrigin = visualTransform.localScale;
        }

        void OnCollisionStay2D(Collision2D collision)
        {
            var pc = collision.gameObject.GetComponent<PlayerController>();
            if (pc == null) return;

            if (!lastBounce.TryGetValue(pc, out float t))
                t = -999f;
            if (Time.time - t < cooldown) return;

            if (pc.transform.position.y < surfaceCollider.bounds.max.y) return;

            lastBounce[pc] = Time.time;

            var rb = pc.Body;
            rb.velocity = new Vector2(rb.velocity.x, 0f);
            rb.AddForce(Vector2.up * bounceForce, ForceMode2D.Impulse);

            if (visualTransform != null)
            {
                visualTransform.DOKill();
                visualTransform.DOScaleY(visualOrigin.y - stretchAmount, squashDuration)
                    .OnComplete(() => visualTransform.DOScaleY(visualOrigin.y, 0.1f).SetEase(Ease.OutBack));
            }
        }
    }
}
