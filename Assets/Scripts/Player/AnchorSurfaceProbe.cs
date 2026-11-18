using UnityEngine;

namespace CGJ2026
{
    /// <summary>A surface an anchoring player can attach to.</summary>
    public readonly struct AnchorSurface
    {
        public readonly Collider2D Collider;
        public readonly Vector2 SnapPosition;
        public readonly Vector2 Normal;

        public AnchorSurface(Collider2D collider, Vector2 snapPosition, Vector2 normal)
        {
            Collider = collider;
            SnapPosition = snapPosition;
            Normal = normal;
        }
    }

    /// <summary>
    /// Finds the nearest anchorable surface around a player by casting
    /// in the four cardinal directions from the body centre.
    /// </summary>
    public sealed class AnchorSurfaceProbe
    {
        static readonly Vector2[] Directions = { Vector2.up, Vector2.down, Vector2.left, Vector2.right };
        const float MinHitDistance = 0.001f;
        const float SnapClearanceFactor = 1.15f;

        readonly RaycastHit2D[] hits = new RaycastHit2D[8];
        readonly LayerMask anchorLayer;
        readonly float reachFactor;

        /// <param name="reachFactor">Probe length as a multiple of (half-width + half-height).</param>
        public AnchorSurfaceProbe(LayerMask anchorLayer, float reachFactor)
        {
            if (reachFactor <= 0f)
                throw new System.ArgumentOutOfRangeException(nameof(reachFactor), "Anchor probe reach must be > 0");

            this.anchorLayer = anchorLayer;
            this.reachFactor = reachFactor;
        }

        public bool TryFind(Bounds bodyBounds, out AnchorSurface surface)
        {
            Vector2 center = bodyBounds.center;
            float halfHeight = bodyBounds.extents.y;
            float reach = (bodyBounds.extents.x + halfHeight) * reachFactor;

            surface = default;
            float bestDistance = float.MaxValue;

            foreach (Vector2 dir in Directions)
            {
                int count = Physics2D.RaycastNonAlloc(center, dir, hits, reach, anchorLayer);
                for (int i = 0; i < count; i++)
                {
                    RaycastHit2D hit = hits[i];
                    if (hit.distance <= MinHitDistance || hit.distance >= bestDistance)
                        continue;

                    bestDistance = hit.distance;
                    Vector2 snapPosition = hit.point + hit.normal * (halfHeight * SnapClearanceFactor);
                    surface = new AnchorSurface(hit.collider, snapPosition, hit.normal);
                }
            }

            return surface.Collider != null;
        }
    }
}
