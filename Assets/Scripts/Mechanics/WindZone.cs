using System.Collections.Generic;
using UnityEngine;

namespace CGJ2026
{
    /// <summary>Pushes every player inside the trigger with a constant force and shows drifting particles.</summary>
    [RequireComponent(typeof(Collider2D))]
    public class WindZone : MonoBehaviour
    {
        static readonly Color ParticleColor = new(0.85f, 0.9f, 1f, 0.6f);

        [SerializeField] Vector2 windForce = new(15f, 5f);
        [SerializeField] float particleRate = 8f;
        [SerializeField] float particleLifetime = 1.5f;

        Collider2D zone;
        ContactFilter2D overlapFilter;
        readonly List<Collider2D> overlaps = new();
        readonly HashSet<PlayerController> pushedThisStep = new();
        float particleTimer;

        void Awake()
        {
            zone = GetComponent<Collider2D>();
            if (!zone.isTrigger)
                throw new System.Exception("WindZone collider must be set to IsTrigger");
            if (particleRate <= 0f)
                throw new System.Exception("WindZone: particleRate must be > 0");

            overlapFilter = new ContactFilter2D { useTriggers = true };
        }

        void FixedUpdate()
        {
            pushedThisStep.Clear();
            zone.OverlapCollider(overlapFilter, overlaps);
            foreach (var col in overlaps)
            {
                var pc = col.GetComponentInParent<PlayerController>();
                // A player can have several colliders; push each player once per step.
                if (pc != null && pushedThisStep.Add(pc))
                    pc.Body.AddForce(windForce, ForceMode2D.Force);
            }
        }

        void Update()
        {
            particleTimer += Time.deltaTime;
            float interval = 1f / particleRate;
            while (particleTimer >= interval)
            {
                particleTimer -= interval;
                SpawnParticle();
            }
        }

        void SpawnParticle()
        {
            Bounds b = zone.bounds;
            var pos = new Vector2(b.min.x + Random.value * b.size.x, b.min.y + Random.value * b.size.y);
            Vector2 drift = windForce.normalized * Random.Range(2f, 5f);
            Juice.SpawnParticle(pos, Random.Range(0.03f, 0.08f), ParticleColor, drift, particleLifetime);
        }
    }
}
