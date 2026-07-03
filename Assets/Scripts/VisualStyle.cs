using UnityEngine;
using UnityEngine.UI;

namespace CGJ2026
{
    /// <summary>
    /// Runtime visual polish — soft storybook frame + warm atmosphere wash.
    /// All code-generated, zero asset dependency. Drop this on any scene GameObject.
    /// </summary>
    public class VisualStyle : MonoBehaviour
    {
        [Header("Frame")]
        [SerializeField] bool frame = true;
        [SerializeField] Color frameColor = new(0.04f, 0.03f, 0.06f, 1f);
        [SerializeField] float thickness = 14f;

        [Header("Atmosphere")]
        [SerializeField] bool atmosphereOverlay = true;
        [SerializeField] Color overlayColor = new(0.88f, 0.85f, 0.80f, 0.05f);

        void Start()
        {
            if (frame) CreateFrame();
            if (atmosphereOverlay) CreateAtmosphere();
        }

        void CreateFrame()
        {
            var canvas = NewCanvas("FrameCanvas", 100);
            AddBar(canvas, "Top",    0,  Screen.height, Screen.width, thickness, new(0, 1));
            AddBar(canvas, "Bottom", 0, -Screen.height, Screen.width, thickness, new(0, 0));
            AddBar(canvas, "Left",  -Screen.width, 0, thickness, Screen.height, new(0, 0));
            AddBar(canvas, "Right",  Screen.width,  0, thickness, Screen.height, new(1, 0));
        }

        void AddBar(GameObject parent, string name, float x, float y, float w, float h, Vector2 pivot)
        {
            var bar = new GameObject(name, typeof(Image));
            bar.transform.SetParent(parent.transform, false);
            var rt = bar.GetComponent<RectTransform>();
            rt.pivot = pivot;
            rt.anchoredPosition = new Vector2(x, y);
            rt.sizeDelta = new Vector2(w, h);
            bar.GetComponent<Image>().color = frameColor;
            bar.GetComponent<Image>().raycastTarget = false;
        }

        void CreateAtmosphere()
        {
            var canvas = NewCanvas("AtmosphereCanvas", -15);
            var panel = new GameObject("Overlay", typeof(Image));
            panel.transform.SetParent(canvas.transform, false);
            var rt = panel.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.sizeDelta = Vector2.zero;
            panel.GetComponent<Image>().color = overlayColor;
            panel.GetComponent<Image>().raycastTarget = false;
        }

        GameObject NewCanvas(string name, int sortOrder)
        {
            var go = new GameObject(name, typeof(Canvas));
            go.transform.SetParent(transform);
            var canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = sortOrder;
            canvas.gameObject.AddComponent<CanvasScaler>();
            return go;
        }
    }
}
