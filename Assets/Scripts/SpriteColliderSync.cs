using UnityEngine;

namespace CGJ2026
{
    /// <summary>
    /// Keeps a BoxCollider2D aligned with a SpriteRenderer's visible bounds.
    /// Works when resizing via Transform scale, SpriteRenderer.size, or both.
    /// Disable Sync Enabled to hand-edit the collider.
    /// </summary>
    [ExecuteAlways]
    [RequireComponent(typeof(BoxCollider2D))]
    public class SpriteColliderSync : MonoBehaviour
    {
        [SerializeField] bool syncEnabled = true;
        [SerializeField] bool syncOffset = true;
        [SerializeField] SpriteRenderer spriteRenderer;

        BoxCollider2D boxCollider;
        Vector3 lastLossyScale;
        Vector2 lastSpriteSize;
        Sprite lastSprite;

        void Reset()
        {
            boxCollider = GetComponent<BoxCollider2D>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
                spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        void Awake()
        {
            CacheComponents();
            Apply();
        }

        void OnValidate()
        {
            CacheComponents();
            if (syncEnabled)
                Apply();
        }

        void LateUpdate()
        {
            if (!syncEnabled || !HasVisualChanged())
                return;

            Apply();
        }

        void CacheComponents()
        {
            if (boxCollider == null)
                boxCollider = GetComponent<BoxCollider2D>();

            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponent<SpriteRenderer>();
                if (spriteRenderer == null)
                    spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            }
        }

        bool HasVisualChanged()
        {
            if (spriteRenderer == null)
                return false;

            Vector3 scale = transform.lossyScale;
            if (scale != lastLossyScale)
                return true;

            if (spriteRenderer.size != lastSpriteSize)
                return true;

            if (spriteRenderer.sprite != lastSprite)
                return true;

            return false;
        }

        public void Apply()
        {
            if (!syncEnabled)
                return;

            CacheComponents();

            if (boxCollider == null)
                throw new System.Exception($"SpriteColliderSync [{name}]: missing BoxCollider2D");

            if (spriteRenderer == null || spriteRenderer.sprite == null)
                throw new System.Exception($"SpriteColliderSync [{name}]: assign a SpriteRenderer with a sprite");

            Bounds worldBounds = spriteRenderer.bounds;
            Vector3 localMin = transform.InverseTransformPoint(worldBounds.min);
            Vector3 localMax = transform.InverseTransformPoint(worldBounds.max);

            boxCollider.size = new Vector2(
                Mathf.Abs(localMax.x - localMin.x),
                Mathf.Abs(localMax.y - localMin.y));

            if (syncOffset)
            {
                Vector3 localCenter = transform.InverseTransformPoint(worldBounds.center);
                boxCollider.offset = localCenter;
            }

            lastLossyScale = transform.lossyScale;
            lastSpriteSize = spriteRenderer.size;
            lastSprite = spriteRenderer.sprite;
        }
    }
}
