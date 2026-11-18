using System.Collections;
using DG.Tweening;
using UnityEngine;

namespace CGJ2026
{
    /// <summary>
    /// Per-level state: lives, the active checkpoint and respawning both players.
    /// </summary>
    public class LevelManager : MonoBehaviour
    {
        const float DeathCooldown = 0.5f;
        const float RespawnHoldDuration = 0.5f;
        const float ShrinkShare = 0.25f;
        const float GrowOvershoot = 2.5f;
        static readonly Vector3 PlayerBSpawnOffset = Vector3.right * 0.5f;

        public static LevelManager Instance { get; private set; }

        [SerializeField] Transform playerA;
        [SerializeField] Transform playerB;
        [SerializeField] PlayerConfig config;
        [SerializeField] KeyCode respawnKey = KeyCode.R;

        [Header("Respawn Animation")]
        [SerializeField] bool useRespawnAnimation = true;
        [SerializeField] float respawnAnimDuration = 0.35f;

        RespawnTarget targetA;
        RespawnTarget targetB;
        RopeSystem rope;
        Vector3 checkpoint;
        int lives;
        float nextDeathAllowedTime;
        bool isRespawning;

        public int Lives => lives;

        /// <summary>Components of one player that the respawn sequence drives.</summary>
        readonly struct RespawnTarget
        {
            public readonly Transform Root;
            public readonly PlayerController Controller;
            public readonly Rigidbody2D Body;
            public readonly PlayerAnimator Animator;

            public RespawnTarget(Transform root)
            {
                if (root == null)
                    throw new System.Exception("LevelManager: player transform not assigned");

                Root = root;
                Controller = Require<PlayerController>(root);
                Body = Require<Rigidbody2D>(root);
                Animator = Require<PlayerAnimator>(root);
                if (Animator.ScaleRoot == null)
                    throw new System.Exception($"LevelManager: {root.name} PlayerAnimator has no scaleRoot");
            }

            static T Require<T>(Component owner) where T : Component
            {
                var c = owner.GetComponent<T>();
                if (c == null)
                    throw new System.Exception($"LevelManager: {owner.name} is missing {typeof(T).Name}");
                return c;
            }

            public void Freeze()
            {
                Controller.IsRespawning = true;
                Animator.enabled = false;
                Body.velocity = Vector2.zero;
                Body.bodyType = RigidbodyType2D.Kinematic;
            }

            public void Unfreeze()
            {
                Body.bodyType = RigidbodyType2D.Dynamic;
                Animator.enabled = true;
                Controller.IsRespawning = false;
            }

            public void PlaceAt(Vector3 position)
            {
                Root.position = position;
                Controller.ResetToHuman();
            }
        }

        void Awake()
        {
            if (Instance != null && Instance != this)
                throw new System.Exception("LevelManager: more than one instance in the scene");
            Instance = this;

            if (config == null)
                throw new System.Exception("LevelManager: config not assigned");

            targetA = new RespawnTarget(playerA);
            targetB = new RespawnTarget(playerB);
            lives = config.MaxLives;
            checkpoint = Vector3.Lerp(playerA.position, playerB.position, 0.5f);
        }

        void Start()
        {
            rope = FindObjectOfType<RopeSystem>();
            if (rope == null)
                throw new System.Exception("LevelManager: no RopeSystem in the scene");
        }

        void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        void OnEnable()
        {
            EventBus.On(E.PlayerFell, OnPlayerFell);
        }

        void OnDisable()
        {
            EventBus.Off(E.PlayerFell, OnPlayerFell);
        }

        void Update()
        {
            if (Time.timeScale > 0f && Input.GetKeyDown(respawnKey))
                RespawnPlayers();
        }

        public void SetCheckpoint(Vector3 position)
        {
            checkpoint = position;
        }

        /// <summary>Respawns both players at the active checkpoint. Ignored while a respawn is already running.</summary>
        public void RespawnPlayers()
        {
            if (isRespawning)
                return;

            if (useRespawnAnimation)
                StartCoroutine(RespawnAnimated());
            else
                PlacePlayersAtCheckpoint();
        }

        void OnPlayerFell(object _)
        {
            // Both players often hit the same hazard on consecutive frames; count that as one death.
            if (isRespawning || Time.time < nextDeathAllowedTime)
                return;
            nextDeathAllowedTime = Time.time + DeathCooldown;

            lives--;
            EventBus.Emit(E.LivesChanged, lives);
            Juice.ShakeHeavy();

            if (lives <= 0)
            {
                EventBus.Emit(E.GameOver);
                return;
            }

            RespawnPlayers();
        }

        void PlacePlayersAtCheckpoint()
        {
            Vector3 spawn = checkpoint + Vector3.up * config.RespawnHeightOffset;
            targetA.PlaceAt(spawn);
            targetB.PlaceAt(spawn + PlayerBSpawnOffset);
            EventBus.Emit(E.PlayerRespawned);
        }

        IEnumerator RespawnAnimated()
        {
            isRespawning = true;

            Transform scaleA = targetA.Animator.ScaleRoot;
            Transform scaleB = targetB.Animator.ScaleRoot;
            Vector3 originalScaleA = scaleA.localScale;
            Vector3 originalScaleB = scaleB.localScale;

            rope.SetVisible(false);
            targetA.Freeze();
            targetB.Freeze();

            float shrinkDuration = respawnAnimDuration * ShrinkShare;
            scaleA.DOScale(Vector3.zero, shrinkDuration).SetUpdate(true);
            scaleB.DOScale(Vector3.zero, shrinkDuration).SetUpdate(true);
            yield return new WaitForSecondsRealtime(shrinkDuration);

            PlacePlayersAtCheckpoint();
            // ResetToHuman made the bodies dynamic again; keep them frozen until they reappear.
            targetA.Body.bodyType = RigidbodyType2D.Kinematic;
            targetB.Body.bodyType = RigidbodyType2D.Kinematic;

            yield return new WaitForSecondsRealtime(RespawnHoldDuration);

            AudioManager.Instance.PlayRespawn();
            float growDuration = respawnAnimDuration - shrinkDuration;
            scaleA.DOScale(originalScaleA, growDuration).SetEase(Ease.OutBack, GrowOvershoot).SetUpdate(true);
            scaleB.DOScale(originalScaleB, growDuration).SetEase(Ease.OutBack, GrowOvershoot).SetUpdate(true);
            yield return new WaitForSecondsRealtime(growDuration);

            targetA.Unfreeze();
            targetB.Unfreeze();
            rope.SetVisible(true);

            isRespawning = false;
        }
    }
}
