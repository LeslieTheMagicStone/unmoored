using TMPro;
using UnityEngine;

namespace CGJ2026
{
    public class HUD : MonoBehaviour
    {
        Canvas canvas;
        TMP_Text livesText;

        void Awake()
        {
            CreateCanvas();
        }

        void CreateCanvas()
        {
            var canvasGo = new GameObject("HUD Canvas");
            canvasGo.transform.SetParent(transform);
            canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10;
            canvasGo.AddComponent<UnityEngine.UI.CanvasScaler>();

            var textGo = new GameObject("LivesText");
            textGo.transform.SetParent(canvasGo.transform, false);
            livesText = textGo.AddComponent<TextMeshProUGUI>();
            livesText.fontSize = 36;
            livesText.alignment = TextAlignmentOptions.TopRight;
            livesText.color = Color.white;
            var rect = livesText.rectTransform;
            rect.anchorMin = new Vector2(1, 1);
            rect.anchorMax = new Vector2(1, 1);
            rect.pivot = new Vector2(1, 1);
            rect.anchoredPosition = new Vector2(-20, -20);
            rect.sizeDelta = new Vector2(200, 50);
        }

        void OnEnable()
        {
            var lm = LevelManager.Instance;
            if (lm != null)
                UpdateDisplay(lm.Lives);

            EventBus.On(E.LivesChanged, OnLivesChanged);
        }

        void OnDisable()
        {
            EventBus.Off(E.LivesChanged);
        }

        void OnLivesChanged(object data)
        {
            UpdateDisplay((int)data);
        }

        void UpdateDisplay(int lives)
        {
            if (livesText != null)
            {
                string label = LocalizationManager.Instance != null
                    ? LocalizationManager.Instance.Get("hud_lives")
                    : "Lives";
                livesText.text = $"{label}: {lives}";
            }
        }
    }
}
