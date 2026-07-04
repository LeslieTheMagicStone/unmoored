using UnityEngine;

namespace CGJ2026
{
    /// <summary>
    /// Visual reference line showing max rope length from this transform.
    /// Place in the scene to check swing reach while building levels.
    /// </summary>
    [ExecuteAlways]
    public class RopeLengthGuide : MonoBehaviour
    {
        static Material sharedMaterial;

        [SerializeField] PlayerConfig config;
        [SerializeField] Vector2 endpointDirection = Vector2.right;
        [SerializeField] Color ropeColor = new(0.8f, 0.65f, 0.2f, 1f);
        [SerializeField] float ropeWidth = 0.06f;
        [SerializeField] int sortingOrder = 5;
        [SerializeField] bool showEndGizmos = true;

        LineRenderer lineRenderer;

        void OnEnable()
        {
            Refresh();
        }

        void OnValidate()
        {
            Refresh();
        }

        void Update()
        {
            if (!Application.isPlaying)
                Refresh();
        }

        public void Refresh()
        {
            EnsureLineRenderer();
            float length = ResolveMaxLength();
            Vector3 start = transform.position;
            Vector3 end = start + (Vector3)(endpointDirection.normalized * length);

            lineRenderer.positionCount = 2;
            lineRenderer.SetPosition(0, start);
            lineRenderer.SetPosition(1, end);
        }

        void EnsureLineRenderer()
        {
            if (lineRenderer == null)
                lineRenderer = GetComponent<LineRenderer>();
            if (lineRenderer == null)
                lineRenderer = gameObject.AddComponent<LineRenderer>();

            if (sharedMaterial == null)
                sharedMaterial = new Material(Shader.Find("Sprites/Default"));

            lineRenderer.useWorldSpace = true;
            lineRenderer.material = sharedMaterial;
            lineRenderer.startColor = lineRenderer.endColor = ropeColor;
            lineRenderer.startWidth = lineRenderer.endWidth = ropeWidth;
            lineRenderer.sortingOrder = sortingOrder;
            lineRenderer.textureMode = LineTextureMode.Tile;
        }

        float ResolveMaxLength()
        {
            if (config != null)
                return config.MaxRopeLength;

            var loaded = Resources.Load<PlayerConfig>("PlayerConfig");
            if (loaded != null)
                return loaded.MaxRopeLength;

            throw new System.Exception(
                "RopeLengthGuide: assign PlayerConfig or place PlayerConfig in Resources/PlayerConfig");
        }

        void OnDrawGizmosSelected()
        {
            if (!showEndGizmos)
                return;

            float length = ResolveMaxLength();
            Vector3 end = transform.position + (Vector3)(endpointDirection.normalized * length);

            Gizmos.color = new Color(1f, 0.85f, 0.2f, 0.9f);
            Gizmos.DrawWireSphere(transform.position, 0.12f);
            Gizmos.DrawWireSphere(end, 0.12f);
            Gizmos.DrawLine(transform.position, end);
        }
    }
}
