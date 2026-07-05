using DG.Tweening;
using UnityEngine;

namespace CGJ2026
{
    public class GameHints : MonoBehaviour
    {
        [SerializeField] CanvasGroup p1Hint;
        [SerializeField] CanvasGroup p2Hint;
        [SerializeField] CanvasGroup restartHint;

        [SerializeField] KeyCode[] p1Keys = { KeyCode.A, KeyCode.D, KeyCode.W, KeyCode.E };
        [SerializeField] KeyCode[] p2Keys = { KeyCode.LeftArrow, KeyCode.RightArrow, KeyCode.UpArrow, KeyCode.Keypad1 };

        [SerializeField] float fadeDuration = 0.8f;

        bool p1Dismissed;
        bool p2Dismissed;
        bool restartDismissed;

        void Start()
        {
            if (p1Hint != null) p1Hint.alpha = 1f;
            if (p2Hint != null) p2Hint.alpha = 1f;
            if (restartHint != null) restartHint.alpha = 1f;
        }

        void OnEnable()
        {
            EventBus.On(E.PlayerRespawned, OnPlayerRespawned);
        }

        void OnDisable()
        {
            EventBus.Off(E.PlayerRespawned, OnPlayerRespawned);
        }

        void Update()
        {
            if (!p1Dismissed)
            {
                if (AnyKeyDown(p1Keys))
                {
                    p1Dismissed = true;
                    if (p1Hint != null)
                        p1Hint.DOFade(0f, fadeDuration).SetUpdate(true);
                }
            }

            if (!p2Dismissed)
            {
                if (AnyKeyDown(p2Keys))
                {
                    p2Dismissed = true;
                    if (p2Hint != null)
                        p2Hint.DOFade(0f, fadeDuration).SetUpdate(true);
                }
            }
        }

        void OnPlayerRespawned(object _)
        {
            if (!restartDismissed)
            {
                restartDismissed = true;
                if (restartHint != null)
                    restartHint.DOFade(0f, fadeDuration).SetUpdate(true);
            }
        }

        bool AnyKeyDown(KeyCode[] keys)
        {
            foreach (var k in keys)
                if (Input.GetKeyDown(k))
                    return true;
            return false;
        }
    }
}
