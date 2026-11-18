using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace CGJ2026
{
    /// <summary>
    /// Persistent singleton that owns the scene flow (MainMenu, levels, GameOver),
    /// the fade-to-black transition and the run timer.
    /// Every scene carries a copy so any scene can be played directly; extra copies destroy themselves.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        const float FadeOutDuration = 0.3f;
        const float FadeInDuration = 0.8f;
        const int OverlaySortingOrder = 100;

        public static GameManager Instance { get; private set; }

        /// <summary>True when the last run ended by clearing every level.</summary>
        public static bool IsGameClear { get; private set; }
        /// <summary>Run time of the last finished run, in seconds.</summary>
        public static float ElapsedTime { get; private set; }

        [SerializeField] string mainMenuScene = "MainMenu";
        [SerializeField] string gameOverScene = "GameOver";
        [SerializeField] string[] levelScenes = { "Game" };

        CanvasGroup overlay;
        int currentLevel;
        float runTime;
        bool timing;
        bool transitioning;

        public static string FormatTime(float seconds)
        {
            int m = (int)(seconds / 60);
            int s = (int)(seconds % 60);
            int cs = (int)(seconds * 100 % 100);
            return $"{m:D2}:{s:D2}:{cs:D2}";
        }

        public static void QuitGame()
        {
            Time.timeScale = 1f;
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        public void LoadMainMenu()
        {
            IsGameClear = false;
            StartTransition(mainMenuScene);
        }

        public void LoadGame()
        {
            IsGameClear = false;
            runTime = 0f;
            ElapsedTime = 0f;
            timing = true;
            currentLevel = 0;
            StartTransition(levelScenes[0]);
        }

        void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            if (levelScenes.Length == 0)
                throw new System.Exception("GameManager: levelScenes is empty");

            Instance = this;
            DontDestroyOnLoad(gameObject);
            overlay = CreateOverlay();
            overlay.alpha = 1f;
        }

        void Start()
        {
            if (Instance != this)
                return;
            StartCoroutine(FadeInNextFrame());
        }

        void OnEnable()
        {
            // A duplicate copy is already scheduled for destruction in Awake; keep it off the bus.
            if (Instance != this)
                return;
            EventBus.On(E.LevelComplete, OnLevelComplete);
            EventBus.On(E.GameOver, OnGameOver);
        }

        void OnDisable()
        {
            EventBus.Off(E.LevelComplete, OnLevelComplete);
            EventBus.Off(E.GameOver, OnGameOver);
        }

        void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        void Update()
        {
            if (timing)
                runTime += Time.deltaTime;
        }

        void OnLevelComplete(object _)
        {
            currentLevel++;
            if (currentLevel < levelScenes.Length)
            {
                StartTransition(levelScenes[currentLevel]);
                return;
            }

            FinishRun(cleared: true);
        }

        void OnGameOver(object _)
        {
            FinishRun(cleared: false);
        }

        void FinishRun(bool cleared)
        {
            timing = false;
            ElapsedTime = runTime;
            IsGameClear = cleared;
            StartTransition(gameOverScene);
        }

        void StartTransition(string sceneName)
        {
            // A second request while fading (e.g. double-clicking a button) would load twice.
            if (transitioning)
                return;
            StartCoroutine(Transition(sceneName));
        }

        IEnumerator Transition(string sceneName)
        {
            transitioning = true;

            overlay.DOKill();
            overlay.DOFade(1f, FadeOutDuration).SetUpdate(true);
            yield return new WaitForSecondsRealtime(FadeOutDuration);

            Time.timeScale = 1f;
            SceneManager.LoadScene(sceneName);
            // Wait for the new scene's first frame so its Start() work is hidden behind the overlay.
            yield return null;
            yield return null;

            overlay.DOFade(0f, FadeInDuration).SetUpdate(true);
            transitioning = false;
        }

        IEnumerator FadeInNextFrame()
        {
            yield return new WaitForEndOfFrame();
            overlay.DOFade(0f, FadeInDuration).SetUpdate(true);
        }

        CanvasGroup CreateOverlay()
        {
            var canvasGo = new GameObject("SceneTransitionOverlay");
            canvasGo.transform.SetParent(transform);
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = OverlaySortingOrder;
            canvasGo.AddComponent<CanvasScaler>();

            var group = canvasGo.AddComponent<CanvasGroup>();
            group.blocksRaycasts = false;

            var imageGo = new GameObject("Black");
            imageGo.transform.SetParent(canvasGo.transform, false);
            var image = imageGo.AddComponent<Image>();
            image.color = Color.black;
            image.raycastTarget = false;
            var rect = image.rectTransform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.sizeDelta = Vector2.zero;

            return group;
        }
    }
}
