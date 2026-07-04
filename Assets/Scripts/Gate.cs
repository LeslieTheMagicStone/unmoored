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

        void Awake()
        {
            closedScale = transform.localScale;
        }

        public void Open()
        {
            closing = false;
            scaleTween?.Kill();
            scaleTween = transform.DOScaleY(0f, duration).SetEase(Ease.OutQuad);
        }

        public void Close()
        {
            closing = true;
            scaleTween?.Kill();
            scaleTween = transform.DOScaleY(closedScale.y, duration).SetEase(Ease.InQuad)
                .OnComplete(() => closing = false);
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
