using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace CGJ2026
{
    public class MainMenuAnchorPrompt : MonoBehaviour
    {
        [SerializeField] Image p1Image;
        [SerializeField] Sprite p1Inactive;
        [SerializeField] Sprite p1Active;
        [SerializeField] Image p2Image;
        [SerializeField] Sprite p2Inactive;
        [SerializeField] Sprite p2Active;

        [SerializeField] Image p1KeyImage;
        [SerializeField] Sprite p1KeyInactive;
        [SerializeField] Sprite p1KeyActive;
        [SerializeField] Image p2KeyImage;
        [SerializeField] Sprite p2KeyInactive;
        [SerializeField] Sprite p2KeyActive;

        [SerializeField] KeyCode p1AnchorKey = KeyCode.S;
        [SerializeField] KeyCode p2AnchorKey = KeyCode.DownArrow;

        [SerializeField] float startDelay = 0.6f;

        bool p1Anchored;
        bool p2Anchored;
        bool starting;

        void Start()
        {
            ApplyState(p1Image, p1Inactive);
            ApplyState(p1KeyImage, p1KeyInactive);
            ApplyState(p2Image, p2Inactive);
            ApplyState(p2KeyImage, p2KeyInactive);
        }

        void Update()
        {
            if (starting) return;

            if (Input.GetKeyDown(p1AnchorKey))
            {
                p1Anchored = !p1Anchored;
                ApplyState(p1Image, p1Anchored ? p1Active : p1Inactive);
                ApplyState(p1KeyImage, p1Anchored ? p1KeyActive : p1KeyInactive);
                if (p1Anchored)
                    EventBus.Emit(E.PlayerAnchored, this);
                else
                    EventBus.Emit(E.PlayerReleased, this);
            }

            if (Input.GetKeyDown(p2AnchorKey))
            {
                p2Anchored = !p2Anchored;
                ApplyState(p2Image, p2Anchored ? p2Active : p2Inactive);
                ApplyState(p2KeyImage, p2Anchored ? p2KeyActive : p2KeyInactive);
                if (p2Anchored)
                    EventBus.Emit(E.PlayerAnchored, this);
                else
                    EventBus.Emit(E.PlayerReleased, this);
            }

            if (p1Anchored && p2Anchored)
            {
                starting = true;
                StartCoroutine(StartGameSequence());
            }
        }

        void ApplyState(Image img, Sprite sprite)
        {
            if (img == null || sprite == null) return;
            img.sprite = sprite;
            img.SetNativeSize();
        }

        IEnumerator StartGameSequence()
        {
            yield return new WaitForSecondsRealtime(startDelay);
            if (GameManager.Instance != null)
                GameManager.Instance.LoadGame();
        }
    }
}
