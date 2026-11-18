using DG.Tweening;
using UnityEngine;

namespace CGJ2026
{
    /// <summary>Control hints that fade out once each player presses one of their keys, and after the first respawn.</summary>
    public class GameHints : MonoBehaviour
    {
        [SerializeField] CanvasGroup p1Hint;
        [SerializeField] CanvasGroup p2Hint;
        [SerializeField] CanvasGroup restartHint;

        [SerializeField] KeyCode[] p1Keys = { KeyCode.A, KeyCode.D, KeyCode.W, KeyCode.S };
        [SerializeField] KeyCode[] p2Keys = { KeyCode.LeftArrow, KeyCode.RightArrow, KeyCode.UpArrow, KeyCode.DownArrow };

        [SerializeField] float fadeDuration = 0.8f;

        bool p1Dismissed;
        bool p2Dismissed;
        bool restartDismissed;

        void Awake()
        {
            if (p1Hint == null || p2Hint == null || restartHint == null)
                throw new System.Exception("GameHints: all three hint groups must be assigned");
        }

        void Start()
        {
            p1Hint.alpha = 1f;
            p2Hint.alpha = 1f;
            restartHint.alpha = 1f;
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
            if (!p1Dismissed && AnyKeyDown(p1Keys))
            {
                p1Dismissed = true;
                FadeOut(p1Hint);
            }

            if (!p2Dismissed && AnyKeyDown(p2Keys))
            {
                p2Dismissed = true;
                FadeOut(p2Hint);
            }
        }

        void OnPlayerRespawned(object _)
        {
            if (restartDismissed)
                return;
            restartDismissed = true;
            FadeOut(restartHint);
        }

        void FadeOut(CanvasGroup hint)
        {
            hint.DOFade(0f, fadeDuration).SetUpdate(true).SetLink(hint.gameObject);
        }

        static bool AnyKeyDown(KeyCode[] keys)
        {
            foreach (var k in keys)
                if (Input.GetKeyDown(k))
                    return true;
            return false;
        }
    }
}
