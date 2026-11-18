using System.Collections.Generic;
using UnityEngine;

namespace CGJ2026
{
    /// <summary>
    /// Activates when both players are anchored inside its trigger: sets the respawn point,
    /// shows the saved UI and optionally raises the water to <see cref="waterTargetY"/>.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class Checkpoint : MonoBehaviour
    {
        const int RequiredPlayers = 2;

        [SerializeField] Sprite inactiveSprite;
        [SerializeField] Sprite activatedSprite;
        [SerializeField] float respawnHeightOffset = 2f;
        [Tooltip("Optional. Leave empty for checkpoints that do not move the water.")]
        [SerializeField] RisingWater risingWater;
        [SerializeField] float waterTargetY;
        [SerializeField] CheckpointUI ui;

        SpriteRenderer spriteRenderer;
        bool activated;
        readonly HashSet<PlayerController> playersInside = new();
        readonly HashSet<PlayerController> anchoredInside = new();

        void Awake()
        {
            if (!GetComponent<Collider2D>().isTrigger)
                throw new System.Exception("Checkpoint collider must be set to IsTrigger");
            if (ui == null)
                throw new System.Exception($"Checkpoint [{name}]: ui not assigned");

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
            if (pc == null || activated)
                return;

            playersInside.Add(pc);
            // A player can snap into the zone already anchored; count them right away.
            if (pc.IsAnchored)
                anchoredInside.Add(pc);
            Refresh();
        }

        void OnTriggerExit2D(Collider2D other)
        {
            var pc = other.GetComponent<PlayerController>();
            if (pc == null || activated)
                return;

            playersInside.Remove(pc);
            anchoredInside.Remove(pc);
            Refresh();
        }

        void OnPlayerAnchored(object data)
        {
            var pc = (PlayerController)data;
            if (activated || !playersInside.Contains(pc))
                return;

            anchoredInside.Add(pc);
            Refresh();
        }

        void OnPlayerReleased(object data)
        {
            if (!activated && anchoredInside.Remove((PlayerController)data))
                Refresh();
        }

        void Refresh()
        {
            if (anchoredInside.Count >= RequiredPlayers)
            {
                Activate();
                return;
            }

            if (playersInside.Count == 0)
            {
                ui.Hide();
                return;
            }

            bool p1Anchored = false;
            bool p2Anchored = false;
            foreach (var pc in anchoredInside)
            {
                if (pc.PlayerIndex == 0) p1Anchored = true;
                if (pc.PlayerIndex == 1) p2Anchored = true;
            }
            ui.ShowProgress(p1Anchored, p2Anchored);
        }

        void Activate()
        {
            activated = true;
            LevelManager.Instance.SetCheckpoint(transform.position + Vector3.up * respawnHeightOffset);

            if (spriteRenderer != null)
                spriteRenderer.sprite = activatedSprite;

            Juice.CheckpointBurst(transform.position);
            EventBus.Emit(E.CheckpointReached, this);
            ui.ShowSaved();

            if (risingWater != null)
                risingWater.RiseTo(waterTargetY);
        }
    }
}
