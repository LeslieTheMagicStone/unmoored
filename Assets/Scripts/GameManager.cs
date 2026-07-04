using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace CGJ2026
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [SerializeField] string mainMenuScene = "MainMenu";
        [SerializeField] string gameOverScene = "GameOver";
        [SerializeField] string[] levelScenes = { "Game" };

        CanvasGroup overlay;
        int currentLevel;

        public int CurrentLevel => currentLevel;
        public int LevelCount => levelScenes.Length;
        public static bool IsGameClear { get; private set; }

        void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            CreateOverlay();
            overlay.alpha = 1f;
        }

        void Start()
        {
            StartCoroutine(FadeFromBlack());
        }

        IEnumerator FadeFromBlack()
        {
            yield return new WaitForEndOfFrame();
            overlay.DOFade(0f, 0.8f).SetUpdate(true);
        }

        void CreateOverlay()
        {
            var canvasGo = new GameObject("SceneTransitionOverlay");
            canvasGo.transform.SetParent(transform);
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            canvasGo.AddComponent<CanvasScaler>();

            overlay = canvasGo.AddComponent<CanvasGroup>();
            overlay.alpha = 0f;
            overlay.blocksRaycasts = false;

            var imgGo = new GameObject("Black");
            imgGo.transform.SetParent(canvasGo.transform, false);
            var img = imgGo.AddComponent<Image>();
            img.color = Color.black;
            var rect = img.rectTransform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.sizeDelta = Vector2.zero;
        }

        void OnEnable()
        {
            EventBus.On(E.LevelComplete, OnLevelComplete);
            EventBus.On(E.GameOver, OnGameOver);
        }

        void OnDisable()
        {
            EventBus.Off(E.LevelComplete, OnLevelComplete);
            EventBus.Off(E.GameOver, OnGameOver);
        }

        void OnLevelComplete(object _)
        {
            currentLevel++;
            if (currentLevel < levelScenes.Length)
            {
                StartCoroutine(Transition(levelScenes[currentLevel]));
            }
            else
            {
                IsGameClear = true;
                StartCoroutine(Transition(gameOverScene));
            }
        }

        void OnGameOver(object _)
        {
            IsGameClear = false;
            StartCoroutine(Transition(gameOverScene));
        }

        public void LoadMainMenu()
        {
            IsGameClear = false;
            StartCoroutine(Transition(mainMenuScene));
        }

        public void LoadGame()
        {
            IsGameClear = false;
            currentLevel = 0;
            StartCoroutine(Transition(levelScenes[0]));
        }

        public void LoadGameOver()
        {
            StartCoroutine(Transition(gameOverScene));
        }

        IEnumerator Transition(string sceneName)
        {
            if (overlay == null) yield break;

            overlay.DOKill();
            overlay.DOFade(1f, 0.3f).SetUpdate(true);
            yield return new WaitForSecondsRealtime(0.35f);

            SceneManager.LoadScene(sceneName);
            yield return new WaitForEndOfFrame();

            overlay.DOFade(0f, 0.8f).SetUpdate(true);
        }
    }
}
