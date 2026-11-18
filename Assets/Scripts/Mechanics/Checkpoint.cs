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
        [SerializeField] RisingWater risingWater;
        [SerializeField] float waterTargetY;
        [SerializeField] CheckpointUI ui;

        SpriteRenderer spriteRenderer;
        bool activated;
        HashSet<PlayerController> playersInside = new HashSet<PlayerController>();
        HashSet<PlayerController> committedPlayers = new HashSet<PlayerController>();

        void Awake()
        {
            var col = GetComponent<Collider2D>();
            if (!col.isTrigger)
                throw new System.Exception("Checkpoint collider must be set to IsTrigger");

            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
                spriteRenderer.sprite = inactiveSprite;
        }

        void OnEnable()
        {
            EventBus.On(E.PlayerAnchored, OnPlayerAnchored);
            EventBus.On(E.PlayerReleased, OnPlayerReleased);
        }

        void OnDisable()
        {
            EventBus.Off(E.PlayerAnchored, OnPlayerAnchored);
            EventBus.Off(E.PlayerReleased, OnPlayerReleased);
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            var pc = other.GetComponent<PlayerController>();
            if (pc != null)
            {
                playersInside.Add(pc);
                RefreshUI();
            }
        }

        void OnTriggerExit2D(Collider2D other)
        {
            var pc = other.GetComponent<PlayerController>();
            if (pc != null)
            {
                playersInside.Remove(pc);
                committedPlayers.Remove(pc);
                RefreshUI();
            }
        }

        void OnPlayerAnchored(object data)
        {
            if (activated) return;
            var pc = data as PlayerController;
            if (pc == null || !playersInside.Contains(pc)) return;

            committedPlayers.Add(pc);
            RefreshUI();

            if (committedPlayers.Count >= 2)
                Activate();
        }

        void OnPlayerReleased(object data)
        {
            if (activated) return;
            var pc = data as PlayerController;
            if (pc == null) return;

            if (committedPlayers.Remove(pc))
                RefreshUI();
        }

        void Activate()
        {
            activated = true;
            Vector3 pos = transform.position + Vector3.up * respawnHeightOffset;
            LevelManager.Instance?.SetCheckpoint(pos);

            if (spriteRenderer != null)
                spriteRenderer.sprite = activatedSprite;

            Juice.CheckpointBurst(transform.position);
            EventBus.Emit(E.CheckpointReached, 0);

            if (ui != null)
                ui.ShowSaved();

            if (risingWater != null)
                risingWater.RiseTo(waterTargetY);
        }

        void RefreshUI()
        {
            if (ui == null || activated) return;

            bool anyInside = playersInside.Count > 0;
            if (!anyInside)
            {
                ui.Hide();
                return;
            }

            bool p1Committed = false;
            bool p2Committed = false;
            foreach (var pc in committedPlayers)
            {
                if (pc.PlayerIndex == 0) p1Committed = true;
                if (pc.PlayerIndex == 1) p2Committed = true;
            }

            ui.ShowProgress(p1Committed, p2Committed);
        }
    }
}
