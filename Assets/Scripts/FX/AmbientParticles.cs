using UnityEngine;

namespace CGJ2026
{
    /// <summary>Spawns drifting dust motes inside the camera view.</summary>
    public class AmbientParticles : MonoBehaviour
    {
        static readonly Color ParticleColor = new(0.85f, 0.9f, 1f, 0.4f);

        [Header("Particle")]
        [SerializeField] float particleRate = 15f;
        [SerializeField] float particleLifetime = 2f;

        [Header("Drift")]
        [SerializeField] Vector2 driftDirection = new(0.2f, 0.8f);

        Camera cam;
        float particleTimer;

        void Awake()
        {
            cam = Camera.main;
            if (cam == null)
                throw new System.Exception("AmbientParticles: no MainCamera in the scene");
            if (particleRate <= 0f)
                throw new System.Exception("AmbientParticles: particleRate must be > 0");
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
            float halfH = cam.orthographicSize;
            float halfW = halfH * cam.aspect;
            Vector3 camPos = cam.transform.position;
            var pos = new Vector2(
                camPos.x + Random.Range(-halfW, halfW),
                camPos.y + Random.Range(-halfH, halfH));

            Vector2 drift = driftDirection.normalized * Random.Range(2f, 5f);
            Juice.SpawnParticle(pos, Random.Range(0.03f, 0.08f), ParticleColor, drift, particleLifetime);
        }
    }
}
