using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace CGJ2026
{
    /// <summary>
    /// Start screen (also used as the GameOver retry screen): both players press their
    /// anchor key to start, mirroring the in-game "both anchor" checkpoint rule.
    /// </summary>
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

        void Awake()
        {
            Object[] required =
            {
                p1Image, p1Inactive, p1Active, p2Image, p2Inactive, p2Active,
                p1KeyImage, p1KeyInactive, p1KeyActive, p2KeyImage, p2KeyInactive, p2KeyActive
            };
            foreach (var o in required)
            {
                if (o == null)
                    throw new System.Exception("MainMenuAnchorPrompt: every image and sprite must be assigned");
            }
        }

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
                AudioManager.Instance.PlayAnchorToggle(p1Anchored);
            }

            if (Input.GetKeyDown(p2AnchorKey))
            {
                p2Anchored = !p2Anchored;
                ApplyState(p2Image, p2Anchored ? p2Active : p2Inactive);
                ApplyState(p2KeyImage, p2Anchored ? p2KeyActive : p2KeyInactive);
                AudioManager.Instance.PlayAnchorToggle(p2Anchored);
            }

            if (p1Anchored && p2Anchored)
            {
                starting = true;
                StartCoroutine(StartGameSequence());
            }
        }

        static void ApplyState(Image img, Sprite sprite)
        {
            img.sprite = sprite;
            img.SetNativeSize();
        }

        IEnumerator StartGameSequence()
        {
            yield return new WaitForSecondsRealtime(startDelay);
            GameManager.Instance.LoadGame();
        }
    }
}
