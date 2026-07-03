using DG.Tweening;
using UnityEngine;

namespace CGJ2026
{
    public class WindZone : MonoBehaviour
    {
        [SerializeField] Vector2 windForce = new(15f, 5f);
        [SerializeField] float particleRate = 8f;
        [SerializeField] float particleLifetime = 1.5f;

        Collider2D zoneCollider;
        float particleTimer;

        void Awake()
        {
            zoneCollider = GetComponent<Collider2D>();
            if (zoneCollider == null)
                throw new System.Exception("WindZone requires a Collider2D set to IsTrigger");
        }

        void FixedUpdate()
        {
            if (zoneCollider == null) return;

            var results = new Collider2D[8];
            var filter = new ContactFilter2D();
            filter.useTriggers = true;
            int count = Physics2D.OverlapCollider(zoneCollider, filter, results);
            for (int i = 0; i < count; i++)
            {
                var col = results[i];
                var pc = col.GetComponent<PlayerController>();
                if (pc != null)
                    pc.Body.AddForce(windForce, ForceMode2D.Force);
            }
        }

        void Update()
        {
            particleTimer += Time.deltaTime;
            if (particleTimer >= 1f / particleRate)
            {
                particleTimer -= 1f / particleRate;
                SpawnParticle();
            }
        }

        void SpawnParticle()
        {
            Bounds b = zoneCollider.bounds;
            Vector2 pos = new(
                b.min.x + Random.value * b.size.x,
                b.min.y + Random.value * b.size.y
            );

            var d = GameObject.CreatePrimitive(PrimitiveType.Cube);
            d.transform.position = pos;
            d.transform.localScale = Vector3.one * Random.Range(0.03f, 0.08f);
            Object.Destroy(d.GetComponent<Collider>());

            var sr = d.GetComponent<MeshRenderer>();
            sr.material = new Material(Shader.Find("Unlit/Color"));
            sr.material.color = new Color(0.85f, 0.9f, 1f, 0.6f);
            sr.material.DOFade(0f, particleLifetime);

            Vector2 drift = windForce.normalized * Random.Range(2f, 5f);
            d.transform.DOMove(d.transform.position + (Vector3)drift, particleLifetime).SetEase(Ease.OutQuad);
            Object.Destroy(d, particleLifetime + 0.1f);
        }
    }
}
