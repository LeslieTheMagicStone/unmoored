using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CGJ2026
{
    public class PauseUI : MonoBehaviour
    {
        GameObject panel;
        GameObject pauseBtn;
        bool isPaused;

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
                TogglePause();
        }

        void OnEnable()
        {
            CreateUI();
        }

        void CreateUI()
        {
            if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                var esGo = new GameObject("EventSystem");
                esGo.AddComponent<UnityEngine.EventSystems.EventSystem>();
                esGo.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            }

            var canvasGo = new GameObject("PauseCanvas");
            canvasGo.transform.SetParent(transform);
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 20;
            canvasGo.AddComponent<CanvasScaler>();
            canvasGo.AddComponent<GraphicRaycaster>();

            // Dim background
            panel = new GameObject("Panel");
            panel.transform.SetParent(canvasGo.transform, false);
            var panelImg = panel.AddComponent<Image>();
            panelImg.color = new Color(0, 0, 0, 0.7f);
            var panelRect = panel.GetComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.sizeDelta = Vector2.zero;
            panel.SetActive(false);

            // Container for buttons
            var container = new GameObject("Container");
            container.transform.SetParent(panel.transform, false);
            var contRect = container.AddComponent<RectTransform>();
            contRect.anchorMin = new Vector2(0.5f, 0.5f);
            contRect.anchorMax = new Vector2(0.5f, 0.5f);
            contRect.sizeDelta = new Vector2(300, 250);

            // Title
            var titleGo = new GameObject("Title");
            titleGo.transform.SetParent(container.transform, false);
            var title = titleGo.AddComponent<TextMeshProUGUI>();
            title.text = "Paused";
            title.fontSize = 48;
            title.alignment = TextAlignmentOptions.Center;
            title.color = Color.white;
            var titleRect = title.rectTransform;
            titleRect.anchoredPosition = new Vector2(0, 110);
            titleRect.sizeDelta = new Vector2(300, 60);

            CreateButton(container.transform, "ResumeBtn", new Vector2(0, 40), "Resume", Resume);
            CreateButton(container.transform, "RestartBtn", new Vector2(0, -20), "Restart", Restart);
            CreateButton(container.transform, "MenuBtn", new Vector2(0, -80), "Main Menu", GoToMenu);

            // Pause toggle button in top-right corner
            pauseBtn = new GameObject("PauseBtn");
            pauseBtn.transform.SetParent(canvasGo.transform, false);
            var pImg = pauseBtn.AddComponent<Image>();
            pImg.color = new Color(0, 0, 0, 0.4f);
            var pRect = pauseBtn.GetComponent<RectTransform>();
            pRect.anchorMin = new Vector2(0, 1);
            pRect.anchorMax = new Vector2(0, 1);
            pRect.pivot = new Vector2(0, 1);
            pRect.anchoredPosition = new Vector2(15, -15);
            pRect.sizeDelta = new Vector2(40, 40);

            var pBtn = pauseBtn.AddComponent<Button>();
            pBtn.onClick.AddListener(TogglePause);

            var pTextGo = new GameObject("Text");
            pTextGo.transform.SetParent(pauseBtn.transform, false);
            var pText = pTextGo.AddComponent<TextMeshProUGUI>();
            pText.text = "II";
            pText.fontSize = 22;
            pText.alignment = TextAlignmentOptions.Center;
            pText.color = Color.white;
            var pTextRect = pText.rectTransform;
            pTextRect.anchorMin = Vector2.zero;
            pTextRect.anchorMax = Vector2.one;
            pTextRect.sizeDelta = Vector2.zero;
        }

        void CreateButton(Transform parent, string name, Vector2 pos, string label, Action onClick)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var img = go.AddComponent<Image>();
            img.color = new Color(0.2f, 0.2f, 0.2f, 1f);
            var rect = go.GetComponent<RectTransform>();
            rect.anchoredPosition = pos;
            rect.sizeDelta = new Vector2(200, 45);

            var btn = go.AddComponent<Button>();
            btn.onClick.AddListener(() => onClick());

            var textGo = new GameObject("Text");
            textGo.transform.SetParent(go.transform, false);
            var text = textGo.AddComponent<TextMeshProUGUI>();
            text.text = label;
            text.fontSize = 28;
            text.alignment = TextAlignmentOptions.Center;
            text.color = Color.white;
            var textRect = text.rectTransform;
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;
        }

        void TogglePause()
        {
            if (isPaused)
                Resume();
            else
                Pause();
        }

        void Pause()
        {
            isPaused = true;
            panel.SetActive(true);
            Time.timeScale = 0f;
        }

        void Resume()
        {
            isPaused = false;
            panel.SetActive(false);
            Time.timeScale = 1f;
        }

        void Restart()
        {
            Time.timeScale = 1f;
            panel.SetActive(false);
            isPaused = false;
            LevelManager.Instance?.RespawnPlayers();
        }

        void GoToMenu()
        {
            Time.timeScale = 1f;
            GameManager.Instance?.LoadMainMenu();
        }

        void OnDisable()
        {
            Time.timeScale = 1f;
        }
    }
}
