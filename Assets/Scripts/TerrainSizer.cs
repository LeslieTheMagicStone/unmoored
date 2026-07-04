using UnityEngine;

namespace CGJ2026
{
    public enum TerrainSizeUnit
    {
        Pixels,
        PlayerWidths
    }

    /// <summary>
    /// Drives transform scale from pixel or player-width layout values.
    /// Only re-applies when its own width/height fields change, so per-instance Scale edits are kept.
    /// Pair with SpriteColliderSync so colliders follow the sprite after resizing.
    /// </summary>
    [ExecuteAlways]
    [DisallowMultipleComponent]
    public class TerrainSizer : MonoBehaviour
    {
        [Header("Width")]
        [SerializeField] TerrainSizeUnit widthUnit = TerrainSizeUnit.PlayerWidths;
        [SerializeField] float width = 5.6f;

        [Header("Height")]
        [SerializeField] TerrainSizeUnit heightUnit = TerrainSizeUnit.Pixels;
        [SerializeField] float height = 100f;

        [Header("Conversion")]
        [SerializeField] float pixelsPerUnit = 100f;
        [SerializeField] Transform playerReference;
        [SerializeField] float referencePlayerSpriteWorldWidth = 1.224f;

        float cachedWidth;
        float cachedHeight;
        TerrainSizeUnit cachedWidthUnit;
        TerrainSizeUnit cachedHeightUnit;
        float cachedPixelsPerUnit;
        Transform cachedPlayerReference;
        float cachedReferencePlayerSpriteWorldWidth;

        void Awake()
        {
            Apply();
            CacheSizingInputs();
        }

        void OnValidate()
        {
            if (!SizingInputsChanged())
                return;

            Apply();
            CacheSizingInputs();
        }

        [ContextMenu("Apply Size")]
        public void Apply()
        {
            if (pixelsPerUnit <= 0f)
                throw new System.Exception($"TerrainSizer [{name}]: pixelsPerUnit must be > 0");

            float worldWidth = ToWorldUnits(width, widthUnit);
            float worldHeight = ToWorldUnits(height, heightUnit);

            if (worldWidth <= 0f || worldHeight <= 0f)
                throw new System.Exception($"TerrainSizer [{name}]: width and height must resolve to values > 0");

            Vector3 scale = transform.localScale;
            transform.localScale = new Vector3(worldWidth, worldHeight, scale.z);

            var spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null && spriteRenderer.drawMode != SpriteDrawMode.Simple)
                spriteRenderer.size = Vector2.one;

            var colliderSync = GetComponent<SpriteColliderSync>();
            if (colliderSync != null)
                colliderSync.Apply();
        }

        bool SizingInputsChanged()
        {
            return !Mathf.Approximately(width, cachedWidth)
                || !Mathf.Approximately(height, cachedHeight)
                || widthUnit != cachedWidthUnit
                || heightUnit != cachedHeightUnit
                || !Mathf.Approximately(pixelsPerUnit, cachedPixelsPerUnit)
                || playerReference != cachedPlayerReference
                || !Mathf.Approximately(referencePlayerSpriteWorldWidth, cachedReferencePlayerSpriteWorldWidth);
        }

        void CacheSizingInputs()
        {
            cachedWidth = width;
            cachedHeight = height;
            cachedWidthUnit = widthUnit;
            cachedHeightUnit = heightUnit;
            cachedPixelsPerUnit = pixelsPerUnit;
            cachedPlayerReference = playerReference;
            cachedReferencePlayerSpriteWorldWidth = referencePlayerSpriteWorldWidth;
        }

        float ToWorldUnits(float value, TerrainSizeUnit unit)
        {
            if (value <= 0f)
                throw new System.Exception($"TerrainSizer [{name}]: size value must be > 0");

            return unit switch
            {
                TerrainSizeUnit.Pixels => value / pixelsPerUnit,
                TerrainSizeUnit.PlayerWidths => value * ResolvePlayerWorldWidth(),
                _ => throw new System.Exception($"TerrainSizer [{name}]: unsupported unit {unit}")
            };
        }

        float ResolvePlayerWorldWidth()
        {
            if (playerReference != null)
                return MeasureSpriteWorldWidth(playerReference);

            if (referencePlayerSpriteWorldWidth <= 0f)
                throw new System.Exception(
                    $"TerrainSizer [{name}]: assign playerReference or set referencePlayerSpriteWorldWidth > 0");

            return referencePlayerSpriteWorldWidth;
        }

        static float MeasureSpriteWorldWidth(Transform root)
        {
            var renderer = root.GetComponentInChildren<SpriteRenderer>();
            if (renderer == null)
                throw new System.Exception($"TerrainSizer [{root.name}]: playerReference requires SpriteRenderer");

            Sprite sprite = renderer.sprite;
            if (sprite == null)
                throw new System.Exception($"TerrainSizer [{root.name}]: playerReference sprite is not assigned");

            return sprite.rect.width / sprite.pixelsPerUnit * renderer.transform.lossyScale.x;
        }
    }
}
