using DG.Tweening;
using UnityEngine;

namespace CGJ2026
{
    /// <summary>
    /// Shakes for <see cref="holdDuration"/> seconds after a player stands on or anchors to it, then breaks.
    /// A player anchored to it loses the pivot when the collider turns off (PlayerController notices).
    /// Comes back after <see cref="respawnDelay"/> unless one-shot, and always on player respawn.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class CrumblingPlatform : MonoBehaviour
    {
        [SerializeField] float holdDuration = 3f;
        [SerializeField] float respawnDelay = 3f;
        [SerializeField] bool oneShot;
        [SerializeField] Transform visualTransform;

        Collider2D platformCollider;
        SpriteRenderer spriteRenderer;
        Transform shakeTarget;
        Vector3 originalLocalPos;
        Color originalColor;
        Tween respawnCall;
        float timer;
        bool triggered;
        bool broken;

        void Awake()
        {
            platformCollider = GetComponent<Collider2D>();
            shakeTarget = visualTransform != null ? visualTransform : transform;
            spriteRenderer = shakeTarget.GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
                throw new System.Exception($"CrumblingPlatform [{name}]: needs a SpriteRenderer on {shakeTarget.name}");

            originalLocalPos = shakeTarget.localPosition;
            originalColor = spriteRenderer.color;
        }

        void OnEnable()
        {
            EventBus.On(E.PlayerAnchored, OnPlayerAnchored);
            EventBus.On(E.PlayerRespawned, OnPlayerRespawned);
        }

        void OnDisable()
        {
            EventBus.Off(E.PlayerAnchored, OnPlayerAnchored);
            EventBus.Off(E.PlayerRespawned, OnPlayerRespawned);
        }

        void OnDestroy()
        {
            respawnCall?.Kill();
            shakeTarget.DOKill();
            spriteRenderer.DOKill();
        }

        void OnCollisionStay2D(Collision2D collision)
        {
            if (triggered)
                return;

            var pc = collision.gameObject.GetComponent<PlayerController>();
            // Only standing on top counts; brushing the side does not start the timer.
            if (pc != null && pc.transform.position.y >= platformCollider.bounds.max.y)
                Trigger();
        }

        void OnPlayerAnchored(object data)
        {
            if (!triggered && ((PlayerController)data).AnchoredCollider == platformCollider)
                Trigger();
        }

        void OnPlayerRespawned(object _)
        {
            if (triggered)
                Restore();
        }

        void FixedUpdate()
        {
            if (!triggered || broken)
                return;

            timer += Time.fixedDeltaTime;
            if (timer >= holdDuration)
                Break();
        }

        void Trigger()
        {
            triggered = true;
            timer = 0f;
            shakeTarget.DOShakePosition(holdDuration, 0.08f, 20, 90, false, true);
        }

        void Break()
        {
            broken = true;
            platformCollider.enabled = false;
            AudioManager.Instance.PlayCrumble();
            Juice.ShakeMedium();

            shakeTarget.DOKill();
            shakeTarget.DOShakePosition(0.2f, 0.12f, 30, 90);
            spriteRenderer.DOFade(0f, 0.25f).SetDelay(0.1f).OnComplete(() =>
            {
                spriteRenderer.enabled = false;
                if (!oneShot)
                    respawnCall = DOVirtual.DelayedCall(respawnDelay, Restore);
            });
        }

        void Restore()
        {
            respawnCall?.Kill();
            respawnCall = null;
            shakeTarget.DOKill();
            spriteRenderer.DOKill();

            shakeTarget.localPosition = originalLocalPos;
            spriteRenderer.enabled = true;
            spriteRenderer.color = originalColor;
            platformCollider.enabled = true;
            triggered = false;
            broken = false;
            timer = 0f;
        }
    }
}
