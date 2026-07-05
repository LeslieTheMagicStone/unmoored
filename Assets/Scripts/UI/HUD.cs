using UnityEngine;

namespace CGJ2026
{
    public class HUD : MonoBehaviour
    {
        void Awake()
        {
            var canvasGo = new GameObject("HUD Canvas");
            canvasGo.transform.SetParent(transform);
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10;
            canvasGo.AddComponent<UnityEngine.UI.CanvasScaler>();
        }
    }
}
