using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;

namespace CGJ2026
{
    /// <summary>
    /// Plays a VideoClip as intro, then loads the next scene.
    /// After skipDelay seconds, any key press skips the video and transitions immediately.
    /// </summary>
    public class MainMenuIntro : MonoBehaviour
    {
        [Header("Scene")]
        [SerializeField] string nextSceneName = "MainMenu";

        [Header("Video")]
        [SerializeField] VideoClip videoClip;
        [SerializeField] float skipDelay = 3f;
        [SerializeField] float fadeDuration = 0.5f;

        RawImage display;
        VideoPlayer player;
        RenderTexture rt;
        bool canSkip;
        bool transitioning;

        void Awake()
        {
            SetupCanvas();
            CreateVideoPlayer();
        }

        void Start()
        {
            player.Play();
            StartCoroutine(EnableSkip());
        }

        void Update()
        {
            if (!transitioning && canSkip && Input.anyKeyDown)
                Skip();
        }

        void SetupCanvas()
        {
            var canvasGo = new GameObject("VideoCanvas");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 9999;
            canvasGo.AddComponent<CanvasScaler>();
            canvasGo.AddComponent<GraphicRaycaster>();

            var rawGo = new GameObject("VideoDisplay");
            rawGo.transform.SetParent(canvasGo.transform, false);

            display = rawGo.AddComponent<RawImage>();
            display.raycastTarget = false;

            var rect = display.rectTransform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        void CreateVideoPlayer()
        {
            if (videoClip == null)
                throw new System.Exception("MainMenuIntro: videoClip not assigned");

            player = gameObject.AddComponent<VideoPlayer>();
            player.playOnAwake = false;
            player.clip = videoClip;
            rt = new RenderTexture((int)videoClip.width, (int)videoClip.height, 0);

            player.targetTexture = rt;
            player.renderMode = VideoRenderMode.RenderTexture;
            player.audioOutputMode = VideoAudioOutputMode.Direct;
            player.isLooping = false;
            player.loopPointReached += OnVideoEnd;

            display.texture = rt;
        }

        void OnVideoEnd(VideoPlayer _)
        {
            player.loopPointReached -= OnVideoEnd;
            Skip();
        }

        IEnumerator EnableSkip()
        {
            yield return new WaitForSecondsRealtime(skipDelay);
            canSkip = true;
        }

        void Skip()
        {
            if (transitioning) return;
            transitioning = true;

            StopAllCoroutines();
            StartCoroutine(TransitionOut());
        }

        IEnumerator TransitionOut()
        {
            float elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                var c = display.color;
                c.a = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
                display.color = c;
                yield return null;
            }

            SceneManager.LoadScene(nextSceneName);
        }

        void OnDestroy()
        {
            if (rt != null)
            {
                rt.Release();
                Destroy(rt);
            }
        }
    }
}
