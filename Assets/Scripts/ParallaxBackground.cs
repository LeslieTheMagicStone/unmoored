using UnityEngine;

namespace CGJ2026
{
    [ExecuteAlways]
    public class ParallaxBackground : MonoBehaviour
    {
        [SerializeField] Transform camTransform;
        [Range(0f, 1f)]
        [SerializeField] float followRatio = 0.9f;

        Vector3 camStart;
        Vector3 bgStart;

        void Start()
        {
            snap();
        }

        void LateUpdate()
        {
            var cam = camTransform ? camTransform : Camera.main ? Camera.main.transform : null;
            if (cam == null) return;
            Vector3 delta = cam.position - camStart;
            transform.position = bgStart + delta * followRatio;
        }

        void OnValidate()
        {
            if (!Application.isPlaying)
                snap();
        }

        void snap()
        {
            var cam = camTransform ? camTransform : Camera.main ? Camera.main.transform : null;
            if (cam == null) return;
            camStart = cam.position;
            bgStart = transform.position;
        }
    }
}
