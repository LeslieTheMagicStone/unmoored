using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CGJ2026
{
    public class MainMenuUI : MonoBehaviour
    {
        void Start()
        {
            if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                var esGo = new GameObject("EventSystem");
                esGo.AddComponent<UnityEngine.EventSystems.EventSystem>();
                esGo.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            }

            var canvasGo = new GameObject("Canvas");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGo.AddComponent<CanvasScaler>();
            canvasGo.AddComponent<GraphicRaycaster>();

            var titleGo = new GameObject("Title");
            titleGo.transform.SetParent(canvasGo.transform, false);
            var title = titleGo.AddComponent<TextMeshProUGUI>();
            title.text = LocalizationManager.Instance != null
                ? LocalizationManager.Instance.Get("mainmenu_title")
                : "My Game";
            title.fontSize = 64;
            title.alignment = TextAlignmentOptions.Center;
            title.color = Color.white;
            var titleRect = title.rectTransform;
            titleRect.anchorMin = new Vector2(0.5f, 0.7f);
            titleRect.anchorMax = new Vector2(0.5f, 0.7f);
            titleRect.sizeDelta = new Vector2(400, 80);

            CreateButton(canvasGo.transform, "PlayBtn", new Vector2(0.5f, 0.45f), "mainmenu_play",
                () => GameManager.Instance.LoadGame());
            CreateButton(canvasGo.transform, "QuitBtn", new Vector2(0.5f, 0.3f), "mainmenu_quit",
                () => Application.Quit());
        }

        void CreateButton(Transform parent, string name, Vector2 anchor, string locKey, UnityEngine.Events.UnityAction onClick)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var img = go.AddComponent<Image>();
            img.color = new Color(0.2f, 0.2f, 0.2f, 1f);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.sizeDelta = new Vector2(200, 50);

            var btn = go.AddComponent<Button>();
            btn.onClick.AddListener(onClick);

            var textGo = new GameObject("Text");
            textGo.transform.SetParent(go.transform, false);
            var text = textGo.AddComponent<TextMeshProUGUI>();
            text.text = LocalizationManager.Instance != null
                ? LocalizationManager.Instance.Get(locKey)
                : name;
            text.fontSize = 28;
            text.alignment = TextAlignmentOptions.Center;
            text.color = Color.white;
            var textRect = text.rectTransform;
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;
        }
    }
}
