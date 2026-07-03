using UnityEngine;
using UnityEngine.SceneManagement;

namespace CGJ2026
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [SerializeField] string mainMenuScene = "MainMenu";
        [SerializeField] string gameScene = "Game";
        [SerializeField] string gameOverScene = "GameOver";

        void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        void OnEnable()
        {
            EventBus.On(E.LevelComplete, OnLevelComplete);
            EventBus.On(E.GameOver, OnGameOver);
        }

        void OnDisable()
        {
            EventBus.Off(E.LevelComplete);
            EventBus.Off(E.GameOver);
        }

        void OnLevelComplete(object _) => LoadGameOver();
        void OnGameOver(object _) => LoadGameOver();

        public void LoadMainMenu() => SceneManager.LoadScene(mainMenuScene);
        public void LoadGame() => SceneManager.LoadScene(gameScene);
        public void LoadGameOver() => SceneManager.LoadScene(gameOverScene);
    }
}
