using DG.Tweening;
using UnityEngine;

namespace CGJ2026
{
    public static class Juice
    {
        public static void DeathFlash()
        {
            var go = new GameObject("DeathFlash");
            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 999;
            var img = go.AddComponent<UnityEngine.UI.Image>();
            img.color = new Color(1f, 0f, 0f, 0.6f);
            var rt = img.rectTransform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.sizeDelta = Vector2.zero;

            img.DOFade(0f, 0.4f).SetDelay(0.05f).OnComplete(() => Object.Destroy(go));
        }

        static void SpawnParticle(Vector2 pos, float size, Color color, Vector2 velocity, float lifetime)
        {
            var d = GameObject.CreatePrimitive(PrimitiveType.Cube);
            d.transform.position = pos;
            d.transform.localScale = Vector3.one * size;
            Object.Destroy(d.GetComponent<Collider>());
            var sr = d.GetComponent<MeshRenderer>();
            sr.material.color = color;
            d.transform.DOMove(d.transform.position + (Vector3)velocity, lifetime).SetEase(Ease.OutQuad);
            sr.material.DOFade(0f, lifetime).OnComplete(() => Object.Destroy(d));
        }

        public static void DustPoof(Vector2 worldPos, int count = 5)
        {
            for (int i = 0; i < count; i++)
            {
                Vector2 vel = Random.insideUnitCircle * 2f + Vector2.up * 1.5f;
                SpawnParticle(worldPos, 0.08f, new Color(0.6f, 0.55f, 0.5f), vel, 0.4f);
            }
        }

        public static void CheckpointBurst(Vector2 worldPos)
        {
            for (int i = 0; i < 8; i++)
            {
                float angle = (i / 8f) * Mathf.PI * 2f;
                Vector2 vel = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * 2f;
                SpawnParticle(worldPos, 0.06f, new Color(0.2f, 0.9f, 0.3f), vel, 0.5f);
            }
        }
    }
}
