using TMPro;
using UnityEngine;

namespace CGJ2026
{
    public class HUD : MonoBehaviour
    {
        Canvas canvas;
        TMP_Text livesText;
        TMP_Text levelText;

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

            // Lives — top-right
            var livesGo = new GameObject("LivesText");
            livesGo.transform.SetParent(canvasGo.transform, false);
            livesText = livesGo.AddComponent<TextMeshProUGUI>();
            livesText.fontSize = 36;
            livesText.alignment = TextAlignmentOptions.TopRight;
            livesText.color = Color.white;
            var livesRect = livesText.rectTransform;
            livesRect.anchorMin = new Vector2(1, 1);
            livesRect.anchorMax = new Vector2(1, 1);
            livesRect.pivot = new Vector2(1, 1);
            livesRect.anchoredPosition = new Vector2(-20, -20);
            livesRect.sizeDelta = new Vector2(300, 50);

            // Level — top-left
            var levelGo = new GameObject("LevelText");
            levelGo.transform.SetParent(canvasGo.transform, false);
            levelText = levelGo.AddComponent<TextMeshProUGUI>();
            levelText.fontSize = 36;
            levelText.alignment = TextAlignmentOptions.TopLeft;
            levelText.color = new Color(1f, 1f, 1f, 0.7f);
            var levelRect = levelText.rectTransform;
            levelRect.anchorMin = new Vector2(0, 1);
            levelRect.anchorMax = new Vector2(0, 1);
            levelRect.pivot = new Vector2(0, 1);
            levelRect.anchoredPosition = new Vector2(20, -20);
            levelRect.sizeDelta = new Vector2(300, 50);
        }

        void Start()
        {
            var gm = GameManager.Instance;
            if (gm != null && levelText != null)
            {
                string label = LocalizationManager.Instance != null
                    ? LocalizationManager.Instance.Get("hud_level")
                    : "Level";
                levelText.text = $"{label} {gm.CurrentLevel + 1}/{gm.LevelCount}";
            }

            var lm = LevelManager.Instance;
            if (lm != null)
                UpdateLives(lm.Lives);
        }

        void OnEnable()
        {
            EventBus.On(E.LivesChanged, OnLivesChanged);
        }

        void OnDisable()
        {
            EventBus.Off(E.LivesChanged, OnLivesChanged);
        }

        void OnLivesChanged(object data)
        {
            UpdateLives((int)data);
        }

        void UpdateLives(int lives)
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
