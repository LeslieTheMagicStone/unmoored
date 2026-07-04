using System.Collections.Generic;
using UnityEngine;

namespace CGJ2026
{
    [RequireComponent(typeof(Collider2D))]
    public class Checkpoint : MonoBehaviour
    {
        [SerializeField] Sprite inactiveSprite;
        [SerializeField] Sprite activatedSprite;
        [SerializeField] float respawnHeightOffset = 2f;

        SpriteRenderer spriteRenderer;
        bool activated;
        HashSet<PlayerController> playersInside = new HashSet<PlayerController>();

        void Awake()
        {
            var col = GetComponent<Collider2D>();
            if (!col.isTrigger)
                throw new System.Exception("Checkpoint collider must be set to IsTrigger");

            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
                spriteRenderer.sprite = inactiveSprite;
        }

        void OnEnable()  => EventBus.On(E.PlayerAnchored, OnPlayerAnchored);
        void OnDisable() => EventBus.Off(E.PlayerAnchored, OnPlayerAnchored);

        void OnTriggerEnter2D(Collider2D other)
        {
            var pc = other.GetComponent<PlayerController>();
            if (pc != null)
                playersInside.Add(pc);
        }

        void OnTriggerExit2D(Collider2D other)
        {
            var pc = other.GetComponent<PlayerController>();
            if (pc != null)
                playersInside.Remove(pc);
        }

        void OnPlayerAnchored(object data)
        {
            var pc = data as PlayerController;
            if (pc == null || !playersInside.Contains(pc)) return;
            if (activated) return;

            Vector3 pos = transform.position + Vector3.up * respawnHeightOffset;
            LevelManager.Instance?.SetCheckpoint(pos);

            activated = true;
            if (spriteRenderer != null)
                spriteRenderer.sprite = activatedSprite;

            Juice.CheckpointBurst(transform.position);
            EventBus.Emit(E.CheckpointReached, pc.PlayerIndex);
        }
    }
}
