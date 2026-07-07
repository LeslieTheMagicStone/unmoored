using DG.Tweening;
using UnityEngine;

namespace CGJ2026
{
    public class Gate : MonoBehaviour
    {
        [SerializeField] float duration = 0.3f;

        Vector3 closedScale;
        Tween scaleTween;
        bool closing;
        bool isOpen;

        void Awake()
        {
            closedScale = transform.localScale;
        }

        void OnEnable()
        {
            EventBus.On(E.PlayerRespawned, OnReset);
        }

        void OnDisable()
        {
            EventBus.Off(E.PlayerRespawned, OnReset);
        }

        void OnReset(object _)
        {
            isOpen = false;
            closing = false;
            scaleTween?.Kill();
            AudioManager.Instance?.StopGateLoop();
            transform.localScale = closedScale;
        }

        public void Open()
        {
            if (isOpen) return;
            isOpen = true;
            closing = false;
            scaleTween?.Kill();
            AudioManager.Instance?.StartGateOpenLoop();
            scaleTween = transform.DOScaleY(0f, 0.5f).SetEase(Ease.OutQuad)
                .OnComplete(() => AudioManager.Instance?.StopGateLoop());
        }

        public void Close()
        {
            if (!isOpen) return;
            isOpen = false;
            closing = true;
            scaleTween?.Kill();
            AudioManager.Instance?.StartGateCloseLoop();
            scaleTween = transform.DOScaleY(closedScale.y, duration).SetEase(Ease.Linear)
                .OnComplete(() =>
                {
                    closing = false;
                    AudioManager.Instance?.StopGateLoop();
                });
        }

        void OnCollisionStay2D(Collision2D collision)
        {
            if (!closing) return;

            var pc = collision.gameObject.GetComponent<PlayerController>();
            if (pc != null && pc.IsGrounded)
                EventBus.Emit(E.PlayerFell);
        }
    }
}
