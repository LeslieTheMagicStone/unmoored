using DG.Tweening;
using UnityEngine;

namespace CGJ2026
{
    /// <summary>
    /// A door driven by a <see cref="PressureSwitch"/>: opens by collapsing its height and closes back.
    /// A grounded player caught under a closing gate is crushed.
    /// </summary>
    public class Gate : MonoBehaviour
    {
        [SerializeField] float openDuration = 0.5f;
        [Tooltip("Close duration. Kept under its original name so existing prefab overrides still apply.")]
        [SerializeField] float duration = 0.3f;

        Vector3 closedScale;
        Tween scaleTween;
        bool isOpen;
        bool closing;

        public void Open()
        {
            if (isOpen)
                return;
            isOpen = true;
            closing = false;
            AnimateTo(0f, openDuration, Ease.OutQuad, AudioManager.Instance.StartGateOpenLoop);
        }

        public void Close()
        {
            if (!isOpen)
                return;
            isOpen = false;
            closing = true;
            AnimateTo(closedScale.y, duration, Ease.Linear, AudioManager.Instance.StartGateCloseLoop);
        }

        void Awake()
        {
            closedScale = transform.localScale;
        }

        void OnEnable()
        {
            EventBus.On(E.PlayerRespawned, OnPlayerRespawned);
        }

        void OnDisable()
        {
            EventBus.Off(E.PlayerRespawned, OnPlayerRespawned);
        }

        void OnDestroy()
        {
            scaleTween?.Kill();
        }

        void OnPlayerRespawned(object _)
        {
            scaleTween?.Kill();
            isOpen = false;
            closing = false;
            transform.localScale = closedScale;
            AudioManager.Instance.StopGateLoop();
        }

        void AnimateTo(float scaleY, float duration, Ease ease, System.Action startLoop)
        {
            scaleTween?.Kill();
            startLoop();
            scaleTween = transform.DOScaleY(scaleY, duration).SetEase(ease).OnComplete(() =>
            {
                closing = false;
                AudioManager.Instance.StopGateLoop();
            });
        }

        void OnCollisionStay2D(Collision2D collision)
        {
            if (!closing)
                return;

            var pc = collision.gameObject.GetComponent<PlayerController>();
            if (pc != null && pc.IsGrounded)
                EventBus.Emit(E.PlayerFell, pc);
        }
    }
}
