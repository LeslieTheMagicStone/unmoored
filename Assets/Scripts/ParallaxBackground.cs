using UnityEngine;

namespace CGJ2026
{
    /// <summary>
    /// Distant parallax: background follows camera with a fraction to create depth.
    /// Attach to background sprite, drag main camera into camTransform.
    /// </summary>
    public class ParallaxBackground : MonoBehaviour
    {
        Vector3 camStart;
        Vector3 bgStart;

        void Start()
        {
            if (Camera.main != null)
                camStart = Camera.main.transform.position;
            bgStart = transform.position;
        }

        void LateUpdate()
        {
            if (Camera.main == null) return;
            Vector3 delta = Camera.main.transform.position - camStart;
            transform.position = bgStart + delta * 0.15f;
        }
    }
}
