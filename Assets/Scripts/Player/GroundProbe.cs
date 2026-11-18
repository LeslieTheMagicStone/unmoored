using UnityEngine;

namespace CGJ2026
{
    /// <summary>
    /// Raycast-based ground and wall detection for a box-shaped character.
    /// Plain C# so the probing rules can be read and tuned in one place.
    /// </summary>
    public sealed class GroundProbe
    {
        // Ray origins as fractions of the half-width. Values past 1 let a player
        // stand on a ledge with only the edge of the box over it.
        static readonly float[] FootRayOffsets = { 0f, -0.5f, 0.5f, -1.05f, 1.05f };
        const float FootRaySkin = 0.01f;
        const float FootRayDistance = 0.08f;
        const float MinGroundNormalY = 0.5f;
        const float MaxGroundedRiseSpeed = 0.1f;

        const float WallRaySkin = 0.02f;
        const float WallRayInset = 0.05f;
        const float WallRayReach = 0.12f;

        readonly BoxCollider2D box;
        readonly Rigidbody2D body;
        readonly LayerMask groundLayer;

        public GroundProbe(BoxCollider2D box, Rigidbody2D body, LayerMask groundLayer)
        {
            this.box = box != null ? box : throw new System.ArgumentNullException(nameof(box));
            this.body = body != null ? body : throw new System.ArgumentNullException(nameof(body));
            this.groundLayer = groundLayer;
        }

        /// <summary>
        /// True when a walkable surface is directly below and the body is not moving up
        /// (so the first frames of a jump never count as grounded).
        /// </summary>
        public bool TryGetGround(out RaycastHit2D groundHit)
        {
            groundHit = default;
            if (body.velocity.y > MaxGroundedRiseSpeed)
                return false;

            Bounds b = box.bounds;
            float footY = b.min.y + FootRaySkin;
            for (int i = 0; i < FootRayOffsets.Length; i++)
            {
                var origin = new Vector2(b.center.x + FootRayOffsets[i] * b.extents.x, footY);
                RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, FootRayDistance, groundLayer);
                if (hit.collider != null && hit.normal.y > MinGroundNormalY)
                {
                    groundHit = hit;
                    return true;
                }
            }

            return false;
        }

        /// <summary>Returns -1 for a wall on the left, 1 for the right, 0 for none.</summary>
        public int FindAdjacentWall()
        {
            Bounds b = box.bounds;
            float[] sampleHeights = { b.max.y - WallRayInset, b.center.y, b.min.y + WallRayInset };

            for (int dir = -1; dir <= 1; dir += 2)
            {
                float x = b.center.x + dir * (b.extents.x + WallRaySkin);
                foreach (float y in sampleHeights)
                {
                    if (Physics2D.Raycast(new Vector2(x, y), Vector2.right * dir, WallRayReach, groundLayer))
                        return dir;
                }
            }

            return 0;
        }
    }
}
