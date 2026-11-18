using DG.Tweening;
using UnityEngine;

namespace CGJ2026
{
    /// <summary>One-call game-feel effects: screen shake and small square particles.</summary>
    public static class Juice
    {
        static Material particleMaterial;
        static Sprite particleSprite;

        public static void ShakeLight() => CameraSetup.Shake(0.6f, 0.15f);
        public static void ShakeMedium() => CameraSetup.Shake(1.2f, 0.25f);
        public static void ShakeHeavy() => CameraSetup.Shake(3f, 0.5f);

        public static void DustPoof(Vector2 worldPos, int count = 5)
        {
            var color = new Color(0.6f, 0.55f, 0.5f);
            for (int i = 0; i < count; i++)
            {
                Vector2 drift = Random.insideUnitCircle * 2f + Vector2.up * 1.5f;
                SpawnParticle(worldPos, 0.08f, color, drift, 0.4f);
            }
        }

        public static void CheckpointBurst(Vector2 worldPos, int count = 8)
        {
            var color = new Color(0.2f, 0.9f, 0.3f);
            for (int i = 0; i < count; i++)
            {
                float angle = i / (float)count * Mathf.PI * 2f;
                Vector2 drift = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * 2f;
                SpawnParticle(worldPos, 0.06f, color, drift, 0.5f);
            }
        }

        /// <summary>
        /// Spawns a square that drifts by <paramref name="drift"/> while fading out, then destroys itself.
        /// All particles share one sprite and one material, so spawning allocates no GPU resources.
        /// </summary>
        public static void SpawnParticle(Vector2 position, float size, Color color, Vector2 drift, float lifetime)
        {
            var go = new GameObject("Particle");
            go.transform.position = position;
            go.transform.localScale = Vector3.one * size;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = GetParticleSprite();
            sr.sharedMaterial = GetParticleMaterial();
            sr.color = color;

            go.transform.DOMove((Vector3)(position + drift), lifetime).SetEase(Ease.OutQuad).SetLink(go);
            sr.DOFade(0f, lifetime).SetLink(go).OnComplete(() => Object.Destroy(go));
        }

        static Sprite GetParticleSprite()
        {
            if (particleSprite != null)
                return particleSprite;

            // 1x1 white texture at 1 pixel per unit: a sprite exactly one world unit wide.
            var texture = new Texture2D(1, 1) { filterMode = FilterMode.Point };
            texture.SetPixel(0, 0, Color.white);
            texture.Apply();
            particleSprite = Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
            return particleSprite;
        }

        static Material GetParticleMaterial()
        {
            if (particleMaterial != null)
                return particleMaterial;

            var shader = Shader.Find("Sprites/Default");
            if (shader == null)
                throw new System.Exception("Juice: shader 'Sprites/Default' not found");
            particleMaterial = new Material(shader);
            return particleMaterial;
        }
    }
}
