using UnityEngine;

namespace CGJ2026
{
    [RequireComponent(typeof(Collider2D))]
    public class Checkpoint : MonoBehaviour
    {
        [SerializeField] Color inactiveColor = new(0.4f, 0.4f, 0.4f, 0.5f);
        [SerializeField] Color activatedColor = new(0.2f, 0.8f, 0.3f, 0.8f);
        [SerializeField] float respawnHeightOffset = 2f;

        SpriteRenderer spriteRenderer;
        bool activated;

        void Awake()
        {
            var col = GetComponent<Collider2D>();
            if (!col.isTrigger)
                throw new System.Exception("Checkpoint collider must be set to IsTrigger");

            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
                spriteRenderer.color = inactiveColor;
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (other.GetComponent<PlayerController>() == null) return;

            Vector3 pos = transform.position + Vector3.up * respawnHeightOffset;
            LevelManager.Instance?.SetCheckpoint(pos);

            if (!activated)
            {
                activated = true;
                if (spriteRenderer != null)
                    spriteRenderer.color = activatedColor;

                var pc = other.GetComponent<PlayerController>();
                EventBus.Emit(E.CheckpointReached, pc.PlayerIndex);
            }
        }
    }
}
