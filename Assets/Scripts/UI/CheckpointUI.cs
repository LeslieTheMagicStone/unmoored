using UnityEngine;
using UnityEngine.UI;

namespace CGJ2026
{
    public class CheckpointUI : MonoBehaviour
    {
        [Header("Sprites")]
        [SerializeField] Sprite p1NotReady;
        [SerializeField] Sprite p1Ready;
        [SerializeField] Sprite p2NotReady;
        [SerializeField] Sprite p2Ready;

        [Header("References")]
        [SerializeField] CanvasGroup canvasGroup;
        [SerializeField] Image p1Image;
        [SerializeField] Image p2Image;

        [Header("Timing")]
        [SerializeField] float fadeDuration = 0.3f;
        [SerializeField] float savedHoldDuration = 1.2f;

        float targetAlpha;

        void Start()
        {
            if (canvasGroup != null)
                canvasGroup.alpha = 0f;
            targetAlpha = 0f;
            ApplySprites(false, false);
        }

        void Update()
        {
            if (canvasGroup == null) return;
            float current = canvasGroup.alpha;
            float next = Mathf.MoveTowards(current, targetAlpha, Time.unscaledDeltaTime / fadeDuration);
            if (!Mathf.Approximately(current, next))
                canvasGroup.alpha = next;
        }

        public void ShowProgress(bool p1Committed, bool p2Committed)
        {
            targetAlpha = 1f;
            ApplySprites(p1Committed, p2Committed);
        }

        public void Hide()
        {
            targetAlpha = 0f;
        }

        public void ShowSaved()
        {
            ApplySprites(true, true);
            targetAlpha = 1f;
            Invoke(nameof(FadeOutAfterSave), savedHoldDuration);
        }

        void FadeOutAfterSave()
        {
            targetAlpha = 0f;
        }

        void ApplySprites(bool p1Committed, bool p2Committed)
        {
            if (p1Image != null)
                p1Image.sprite = p1Committed ? p1Ready : p1NotReady;
            if (p2Image != null)
                p2Image.sprite = p2Committed ? p2Ready : p2NotReady;
        }
    }
}
