using DG.Tweening;
using UnityEngine;

namespace CGJ2026
{
    public class Gate : MonoBehaviour
    {
        [SerializeField] float duration = 0.3f;

        Vector3 closedPos;
        Vector3 closedScale;
        float topY;
        float spriteHeight;
        SpriteRenderer sr;
        Tween scaleTween;
        bool closing;

        void Awake()
        {
            sr = GetComponent<SpriteRenderer>();
            closedPos = transform.localPosition;
            closedScale = transform.localScale;
            spriteHeight = sr.size.y;
            topY = closedPos.y + spriteHeight * closedScale.y * 0.5f;
        }

        public void Open()
        {
            closing = false;
            scaleTween?.Kill();
            scaleTween = DOTween.To(
                () => transform.localScale.y,
                y => ApplyScaleY(y),
                0f,
                duration).SetEase(Ease.OutQuad);
        }

        public void Close()
        {
            closing = true;
            scaleTween?.Kill();
            scaleTween = DOTween.To(
                () => transform.localScale.y,
                y => ApplyScaleY(y),
                closedScale.y,
                duration).SetEase(Ease.InQuad)
                .OnComplete(() => closing = false);
        }

        void ApplyScaleY(float y)
        {
            Vector3 s = transform.localScale;
            s.y = y;
            transform.localScale = s;

            Vector3 p = transform.localPosition;
            p.y = topY - spriteHeight * y * 0.5f;
            transform.localPosition = p;
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
