using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace CGJ2026
{
    public class CrumblingPlatform : MonoBehaviour
    {
        [SerializeField] float holdDuration = 2f;
        [SerializeField] float respawnDelay = 3f;
        [SerializeField] bool oneShot;
        [SerializeField] Transform visualTransform;

        Collider2D platformCollider;
        SpriteRenderer spriteRenderer;
        Vector3 originalPos;
        Color originalColor;
        Tween shakeTween;
        float timer;
        bool broken;
        bool triggered;
        readonly List<PlayerController> anchoredPlayers = new List<PlayerController>();

        void Awake()
        {
            platformCollider = GetComponent<Collider2D>();
            spriteRenderer = visualTransform != null ? visualTransform.GetComponent<SpriteRenderer>() : GetComponent<SpriteRenderer>();
            originalPos = visualTransform != null ? visualTransform.localPosition : transform.localPosition;
            if (spriteRenderer != null)
                originalColor = spriteRenderer.color;
        }

        void OnEnable()
        {
            EventBus.On(E.PlayerAnchored, OnPlayerAnchored);
            EventBus.On(E.PlayerReleased, OnPlayerReleased);
            EventBus.On(E.PlayerRespawned, OnPlayerRespawned);
        }

        void OnDisable()
        {
            EventBus.Off(E.PlayerAnchored, OnPlayerAnchored);
            EventBus.Off(E.PlayerReleased, OnPlayerReleased);
            EventBus.Off(E.PlayerRespawned, OnPlayerRespawned);
        }

        void OnCollisionStay2D(Collision2D collision)
        {
            if (broken || triggered) return;
            var pc = collision.gameObject.GetComponent<PlayerController>();
            if (pc == null) return;

            if (pc.transform.position.y < platformCollider.bounds.max.y)
                return;

            Activate();
        }

        void OnPlayerAnchored(object data)
        {
            var pc = data as PlayerController;
            if (pc == null) return;
            if (broken) return;

            if (!PlayerIsOnPlatform(pc))
                return;

            anchoredPlayers.Add(pc);
            if (!triggered)
                Activate();
        }

        void OnPlayerReleased(object data)
        {
            var pc = data as PlayerController;
            if (pc == null) return;
            anchoredPlayers.Remove(pc);
        }

        void OnPlayerRespawned(object _)
        {
            if (broken || triggered)
                Respawn();
        }

        bool PlayerIsOnPlatform(PlayerController pc)
        {
            Bounds b = platformCollider.bounds;
            Vector2 pos = pc.transform.position;
            float margin = 1.0f;
            bool result = pos.x > b.min.x - margin && pos.x < b.max.x + margin
                && pos.y > b.min.y - margin && pos.y < b.max.y + margin;
            return result;
        }

        void Activate()
        {
            triggered = true;
            Transform t = visualTransform != null ? visualTransform : transform;
            shakeTween = t.DOShakePosition(holdDuration, 0.08f, 20, 90, false, true);
        }

        void FixedUpdate()
        {
            if (broken || !triggered)
                return;

            timer += Time.fixedDeltaTime;
            if (timer >= holdDuration)
                Break();
        }

        void Break()
        {
            broken = true;
            shakeTween?.Kill();
            shakeTween = null;
            platformCollider.enabled = false;
            AudioManager.Instance?.PlayCrumble();
            Juice.ShakeMedium();

            for (int i = anchoredPlayers.Count - 1; i >= 0; i--)
                anchoredPlayers[i].ReleaseFromSurface();
            anchoredPlayers.Clear();

            Transform t = visualTransform != null ? visualTransform : transform;
            t.DOShakePosition(0.2f, 0.12f, 30, 90);
            spriteRenderer.DOFade(0f, 0.25f).SetDelay(0.1f).OnComplete(() =>
            {
                spriteRenderer.enabled = false;
                timer = 0f;

                if (!oneShot)
                    DOVirtual.DelayedCall(respawnDelay, Respawn);
            });
        }

        void Respawn()
        {
            broken = false;
            triggered = false;
            platformCollider.enabled = true;
            spriteRenderer.enabled = true;
            spriteRenderer.color = originalColor;
            anchoredPlayers.Clear();
            if (visualTransform != null)
                visualTransform.localPosition = originalPos;
            else
                transform.localPosition = originalPos;
        }
    }
}
