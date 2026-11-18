using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CGJ2026
{
    /// <summary>ESC / on-screen pause menu for the Game scene: resume, restart from checkpoint, back to menu.</summary>
    public class PauseUI : MonoBehaviour
    {
        [SerializeField] GameObject pausePanel;
        [SerializeField] Button pauseBtn;
        [SerializeField] Button resumeBtn;
        [SerializeField] Button restartBtn;
        [SerializeField] Button menuBtn;

        [SerializeField] float hoverScale = 1.1f;
        [SerializeField] float pressScale = 0.95f;
        [SerializeField] float animDuration = 0.15f;

        bool isPaused;

        void Awake()
        {
            if (pausePanel == null || pauseBtn == null || resumeBtn == null || restartBtn == null || menuBtn == null)
                throw new System.Exception("PauseUI: panel and all four buttons must be assigned");
        }

        void Start()
        {
            pausePanel.SetActive(false);

            resumeBtn.onClick.AddListener(Resume);
            restartBtn.onClick.AddListener(Restart);
            menuBtn.onClick.AddListener(GoToMenu);
            pauseBtn.onClick.AddListener(TogglePause);

            AddHoverAnim(pauseBtn);
            AddHoverAnim(resumeBtn);
            AddHoverAnim(restartBtn);
            AddHoverAnim(menuBtn);
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
                TogglePause();
        }

        void TogglePause()
        {
            if (isPaused) Resume();
            else Pause();
        }

        void Pause()
        {
            isPaused = true;
            pausePanel.SetActive(true);
            pauseBtn.gameObject.SetActive(false);
            Time.timeScale = 0f;
            AudioManager.Instance.PlayPause();
            AudioManager.Instance.SetMusicLowPass(true);
        }

        void Resume()
        {
            isPaused = false;
            pausePanel.SetActive(false);
            pauseBtn.gameObject.SetActive(true);
            Time.timeScale = 1f;
            AudioManager.Instance.SetMusicLowPass(false);
        }

        void Restart()
        {
            Resume();
            LevelManager.Instance.RespawnPlayers();
        }

        void GoToMenu()
        {
            Resume();
            GameManager.Instance.LoadMainMenu();
        }

        void OnDisable()
        {
            Time.timeScale = 1f;
        }

        void AddHoverAnim(Button btn)
        {
            var t = btn.transform;
            bool isHovered = false;

            AddTrigger(btn, EventTriggerType.PointerEnter, _ =>
            {
                isHovered = true;
                AudioManager.Instance.PlayUIHover();
                t.DOScale(hoverScale, animDuration).SetUpdate(true);
            });

            AddTrigger(btn, EventTriggerType.PointerExit, _ =>
            {
                isHovered = false;
                t.DOScale(1f, animDuration).SetUpdate(true);
            });

            AddTrigger(btn, EventTriggerType.PointerDown, _ =>
            {
                t.DOScale(pressScale, animDuration).SetUpdate(true);
            });

            AddTrigger(btn, EventTriggerType.PointerUp, _ =>
            {
                AudioManager.Instance.PlayUIClick();
                t.DOScale(isHovered ? hoverScale : 1f, animDuration).SetUpdate(true);
            });
        }

        static void AddTrigger(Button btn, EventTriggerType type, UnityEngine.Events.UnityAction<BaseEventData> action)
        {
            var trigger = btn.GetComponent<EventTrigger>();
            if (trigger == null) trigger = btn.gameObject.AddComponent<EventTrigger>();
            var entry = new EventTrigger.Entry { eventID = type };
            entry.callback.AddListener(action);
            trigger.triggers.Add(entry);
        }
    }
}
