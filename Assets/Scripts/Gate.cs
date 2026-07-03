using DG.Tweening;
using UnityEngine;

namespace CGJ2026
{
    public class Gate : MonoBehaviour
    {
        [SerializeField] float openHeight = 4f;
        [SerializeField] float duration = 0.3f;

        Vector3 closedPos;
        Vector3 openPos;
        Tween moveTween;
        bool closing;

        void Awake()
        {
            closedPos = transform.localPosition;
            openPos = closedPos + Vector3.up * openHeight;
        }

        public void Open()
        {
            closing = false;
            moveTween?.Kill();
            moveTween = transform.DOLocalMove(openPos, duration).SetEase(Ease.OutQuad);
        }

        public void Close()
        {
            closing = true;
            moveTween?.Kill();
            moveTween = transform.DOLocalMove(closedPos, duration).SetEase(Ease.InQuad)
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
