using UnityEngine;

namespace CGJ2026
{
    [ExecuteAlways]
    public class WaterFollow : MonoBehaviour
    {
        [SerializeField] Transform target;
        [SerializeField] float yOffset = -5f;

        void LateUpdate()
        {
            var t = target ? target : Camera.main ? Camera.main.transform : null;
            if (t == null) return;
            var pos = transform.position;
            pos.y = t.position.y + yOffset;
            transform.position = pos;
        }
    }
}
